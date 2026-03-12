using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Application.Common.Pricing;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Quotes;

public sealed record GetQuoteQuery(int PropertyId, int? UnitId, DateOnly CheckInDate, DateOnly CheckOutDate, int Adults, int Children) : IRequest<AppResult<QuoteResultDto>>;

public sealed class GetQuoteQueryValidator : AbstractValidator<GetQuoteQuery>
{
    public GetQuoteQueryValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.Adults).GreaterThan(0);
        RuleFor(x => x.Children).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetQuoteQueryHandler :
    IRequestHandler<GetQuoteQuery, AppResult<QuoteResultDto>>,
    IRequestHandler<SaveQuoteCommand, AppResult<SavedQuoteDto>>,
    IRequestHandler<GetSavedQuotesQuery, AppResult<List<SavedQuoteDto>>>,
    IRequestHandler<GetSavedQuoteDetailQuery, AppResult<SavedQuoteDto>>,
    IRequestHandler<ConvertSavedQuoteToBookingCommand, AppResult<int>>,
    IRequestHandler<PricingSimulationQuery, AppResult<PricingSimulationDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IPropertyFeatureService _features;

    public GetQuoteQueryHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _features = features;
    }

    public async Task<AppResult<QuoteResultDto>> Handle(GetQuoteQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Quotes, ct))
            return AppResult<QuoteResultDto>.Fail("feature_disabled", "El cotizador está desactivado para este hospedaje.");

        var property = await _db.Properties.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (property is null)
            return AppResult<QuoteResultDto>.Fail("forbidden", "Hospedaje inválido o sin acceso.");
        if (!property.IsActive)
            return AppResult<QuoteResultDto>.Fail("property_inactive", "El hospedaje está inactivo y no se puede cotizar.");

        var unitsQuery = _db.Units.AsNoTracking()
            .Where(u => u.PropertyId == request.PropertyId && (u.Property.Account.OwnerUserId == _current.UserId || u.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && u.IsActive && u.Property.IsActive);
        if (request.UnitId.HasValue)
            unitsQuery = unitsQuery.Where(x => x.Id == request.UnitId.Value);

        var units = await unitsQuery.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync(ct);
        var available = new List<QuoteAvailableUnitDto>();
        var globalMessages = new List<string>();

        foreach (var unit in units)
        {
            if (unit.OperationalStatus == UnitOperationalStatus.Maintenance)
            {
                globalMessages.Add($"{unit.Name}: unidad fuera de servicio por mantenimiento.");
                continue;
            }

            var overlapBooking = await _db.Bookings.AsNoTracking()
                .Where(b => b.PropertyId == request.PropertyId && b.UnitId == unit.Id && b.Status != BookingStatus.Cancelled)
                .AnyAsync(b => b.CheckInDate < request.CheckOutDate && request.CheckInDate < b.CheckOutDate, ct);

            var overlapBlock = await _db.BlockedDates.AsNoTracking()
                .Where(x => x.PropertyId == request.PropertyId && x.UnitId == unit.Id)
                .AnyAsync(b => b.DateFrom < request.CheckOutDate && request.CheckInDate < b.DateTo, ct);

            var overlapExternal = await _db.ExternalCalendarEvents.AsNoTracking()
                .Where(x => x.PropertyId == request.PropertyId && x.UnitId == unit.Id && !x.IsCancelled)
                .Where(x => x.ExternalChannelConnection.IsActive)
                .AnyAsync(x => x.StartDate < request.CheckOutDate && request.CheckInDate < x.EndDate, ct);

            if (overlapBooking || overlapBlock || overlapExternal)
            {
                globalMessages.Add($"{unit.Name}: no disponible para el rango solicitado.");
                continue;
            }

            var pricing = await CommercialPricing.CalculateAsync(_db, request.PropertyId, unit.Id, request.CheckInDate, request.CheckOutDate, request.Adults, request.Children, ct);
            available.Add(new QuoteAvailableUnitDto(
                unit.Id,
                unit.Name,
                pricing.BaseAmount,
                pricing.PromotionsAmount,
                pricing.SuggestedNightlyRate,
                pricing.FinalAmount,
                pricing.SuggestedDepositAmount,
                unit.OperationalStatus,
                pricing.Rules,
                pricing.Lines,
                pricing.NightBreakdown,
                pricing.AppliedPromotions));
        }

        var best = available.OrderBy(x => x.Total).FirstOrDefault();
        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        var summary = best is null
            ? "No hay disponibilidad para el rango solicitado."
            : $"{available.Count} unidad(es) disponible(s). Mejor tarifa final: {best.Total:0.00}.";

        return AppResult<QuoteResultDto>.Ok(new QuoteResultDto(
            request.PropertyId,
            request.CheckInDate,
            request.CheckOutDate,
            nights,
            request.Adults,
            request.Children,
            best?.SuggestedNightlyRate ?? 0m,
            best?.BaseAmount ?? 0m,
            best?.PromotionsAmount ?? 0m,
            best?.Total ?? 0m,
            best?.SuggestedDepositAmount ?? 0m,
            available,
            summary,
            globalMessages));
    }

    public async Task<AppResult<SavedQuoteDto>> Handle(SaveQuoteCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.SavedQuotes, ct))
            return AppResult<SavedQuoteDto>.Fail("feature_disabled", "Las cotizaciones guardadas están desactivadas para este hospedaje.");

        var quote = await Handle(new GetQuoteQuery(request.PropertyId, request.UnitId, request.CheckInDate, request.CheckOutDate, request.Adults, request.Children), ct);
        var selected = request.UnitId.HasValue
            ? quote.Data?.AvailableUnits.FirstOrDefault(x => x.UnitId == request.UnitId.Value)
            : quote.Data?.AvailableUnits.OrderBy(x => x.Total).FirstOrDefault();
        if (quote.Data is null || selected is null)
            return AppResult<SavedQuoteDto>.Fail("not_available", "No se pudo guardar la cotización.");

        var entity = new SavedQuote
        {
            PropertyId = request.PropertyId,
            UnitId = selected.UnitId,
            PublicToken = Guid.NewGuid().ToString("N"),
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            Adults = request.Adults,
            Children = request.Children,
            BaseAmount = selected.BaseAmount,
            PromotionsAmount = selected.PromotionsAmount,
            TotalAmount = selected.Total,
            SuggestedDepositAmount = selected.SuggestedDepositAmount,
            Summary = quote.Data.Summary,
            AppliedPromotionNames = string.Join(", ", selected.AppliedPromotions.Select(x => x.Name)),
            GuestName = request.GuestName,
            GuestEmail = request.GuestEmail,
            GuestPhone = request.GuestPhone,
            Status = SavedQuoteStatus.Saved
        };

        _db.SavedQuotes.Add(entity);
        await _db.SaveChangesAsync(ct);
        var unitName = await _db.Units.AsNoTracking().Where(x => x.Id == entity.UnitId).Select(x => x.Name).FirstOrDefaultAsync(ct);
        return AppResult<SavedQuoteDto>.Ok(MapSavedQuote(entity, unitName));
    }

    public async Task<AppResult<List<SavedQuoteDto>>> Handle(GetSavedQuotesQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.SavedQuotes, ct))
            return AppResult<List<SavedQuoteDto>>.Fail("feature_disabled", "Las cotizaciones guardadas están desactivadas para este hospedaje.");

        var query = _db.SavedQuotes.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Include(x => x.Unit)
            .OrderByDescending(x => x.CreatedAtUtc)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(x => (x.GuestName ?? "").Contains(request.Search) || (x.GuestEmail ?? "").Contains(request.Search) || (x.PublicToken ?? "").Contains(request.Search));

        var data = await query.Select(x => new SavedQuoteDto(
            x.Id,
            x.PublicToken,
            x.PropertyId,
            x.UnitId,
            x.Unit != null ? x.Unit.Name : null,
            x.CheckInDate,
            x.CheckOutDate,
            x.Adults,
            x.Children,
            x.BaseAmount,
            x.PromotionsAmount,
            x.TotalAmount,
            x.SuggestedDepositAmount,
            x.AppliedPromotionNames,
            x.GuestName,
            x.GuestEmail,
            x.GuestPhone,
            x.Status,
            x.CreatedAtUtc,
            x.CreatedBookingId,
            $"/public/quote/{x.PublicToken}",
            x.Summary)).ToListAsync(ct);

        return AppResult<List<SavedQuoteDto>>.Ok(data);
    }

    public async Task<AppResult<SavedQuoteDto>> Handle(GetSavedQuoteDetailQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.SavedQuotes, ct))
            return AppResult<SavedQuoteDto>.Fail("feature_disabled", "Las cotizaciones guardadas están desactivadas para este hospedaje.");

        var entity = await _db.SavedQuotes.AsNoTracking()
            .Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == request.SavedQuoteId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        return entity is null
            ? AppResult<SavedQuoteDto>.Fail("not_found", "Cotización no encontrada.")
            : AppResult<SavedQuoteDto>.Ok(MapSavedQuote(entity, entity.Unit?.Name));
    }

    public async Task<AppResult<int>> Handle(ConvertSavedQuoteToBookingCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.SavedQuotes, ct))
            return AppResult<int>.Fail("feature_disabled", "Las cotizaciones guardadas están desactivadas para este hospedaje.");

        var quote = await _db.SavedQuotes.FirstOrDefaultAsync(x => x.Id == request.SavedQuoteId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (quote is null)
            return AppResult<int>.Fail("not_found", "Cotización guardada no encontrada.");
        if (quote.UnitId is null)
            return AppResult<int>.Fail("unit_required", "La cotización no tiene una unidad asociada.");
        if (quote.Status == SavedQuoteStatus.Converted && quote.CreatedBookingId.HasValue)
            return AppResult<int>.Fail("already_converted", "La cotización ya fue convertida a reserva.");

        var overlap = await _db.Bookings.AsNoTracking()
            .AnyAsync(b => b.PropertyId == request.PropertyId && b.UnitId == quote.UnitId.Value && b.Status != BookingStatus.Cancelled && b.CheckInDate < quote.CheckOutDate && quote.CheckInDate < b.CheckOutDate, ct);
        if (overlap)
            return AppResult<int>.Fail("not_available", "La unidad ya no está disponible para convertir esta cotización.");

        var booking = new Booking
        {
            PropertyId = quote.PropertyId,
            UnitId = quote.UnitId.Value,
            GuestId = request.GuestId,
            BookingCode = $"QBK-{DateTime.UtcNow:yyyyMMddHHmmss}-{quote.UnitId}",
            CheckInDate = quote.CheckInDate,
            CheckOutDate = quote.CheckOutDate,
            Adults = quote.Adults,
            Children = quote.Children,
            TotalAmount = quote.TotalAmount,
            BaseAmount = quote.BaseAmount,
            PromotionsAmount = quote.PromotionsAmount,
            ExpectedDepositAmount = quote.SuggestedDepositAmount,
            Status = request.Status,
            Source = BookingSource.Direct,
            Notes = request.Notes?.Trim(),
            CreatedFromQuote = true,
            SavedQuoteId = quote.Id,
            AppliedPromotionNames = quote.AppliedPromotionNames,
            SuggestedNightlyRate = Math.Max(0, (quote.CheckOutDate.DayNumber - quote.CheckInDate.DayNumber)) == 0 ? 0 : Math.Round(quote.TotalAmount / (quote.CheckOutDate.DayNumber - quote.CheckInDate.DayNumber), 2),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OperationalStatus = request.Status == BookingStatus.CheckedIn ? BookingOperationalStatus.CheckedIn : BookingOperationalStatus.PendingCheckIn
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        quote.Status = SavedQuoteStatus.Converted;
        quote.CreatedBookingId = booking.Id;

        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = booking.PropertyId,
            BookingId = booking.Id,
            EventType = BookingEventType.Quoted,
            Title = "Reserva creada desde cotización",
            Detail = $"Cotización #{quote.Id} convertida. Token: {quote.PublicToken}.",
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });

        _db.AuditLogs.Add(new AuditLog
        {
            AccountId = (await _db.Properties.AsNoTracking().Where(x => x.Id == request.PropertyId).Select(x => x.AccountId).FirstAsync(ct)),
            PropertyId = request.PropertyId,
            EntityName = nameof(SavedQuote),
            EntityId = quote.Id,
            Action = "convert_to_booking",
            Summary = $"Cotización {quote.PublicToken} convertida en reserva {booking.BookingCode}",
            UserId = _current.UserId,
            UserName = _current.Email
        });

        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(booking.Id);
    }

    public async Task<AppResult<PricingSimulationDto>> Handle(PricingSimulationQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Quotes, ct))
            return AppResult<PricingSimulationDto>.Fail("feature_disabled", "El cotizador está desactivado para este hospedaje.");

        var unitName = await _db.Units.AsNoTracking()
            .Where(x => x.Id == request.UnitId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(x => x.Name)
            .FirstOrDefaultAsync(ct);
        if (unitName is null)
            return AppResult<PricingSimulationDto>.Fail("not_found", "Unidad no encontrada.");

        var quote = await Handle(new GetQuoteQuery(request.PropertyId, request.UnitId, request.CheckInDate, request.CheckOutDate, request.Adults, request.Children), ct);
        var selected = quote.Data?.AvailableUnits.FirstOrDefault();
        if (quote.Data is null || selected is null)
            return AppResult<PricingSimulationDto>.Fail("not_available", "No fue posible simular la tarifa para esa unidad.");

        return AppResult<PricingSimulationDto>.Ok(new PricingSimulationDto(
            request.PropertyId,
            request.UnitId,
            unitName,
            quote.Data,
            selected.Rules));
    }

    private static SavedQuoteDto MapSavedQuote(SavedQuote entity, string? unitName)
        => new(
            entity.Id,
            entity.PublicToken,
            entity.PropertyId,
            entity.UnitId,
            unitName,
            entity.CheckInDate,
            entity.CheckOutDate,
            entity.Adults,
            entity.Children,
            entity.BaseAmount,
            entity.PromotionsAmount,
            entity.TotalAmount,
            entity.SuggestedDepositAmount,
            entity.AppliedPromotionNames,
            entity.GuestName,
            entity.GuestEmail,
            entity.GuestPhone,
            entity.Status,
            entity.CreatedAtUtc,
            entity.CreatedBookingId,
            $"/public/quote/{entity.PublicToken}",
            entity.Summary);
}
