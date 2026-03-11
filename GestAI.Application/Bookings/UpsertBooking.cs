using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Application.Common.Pricing;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record UpsertBookingCommand(
    int PropertyId,
    int? BookingId,
    int UnitId,
    int GuestId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    decimal TotalAmount,
    string? Notes,
    BookingSource Source = BookingSource.Direct,
    BookingStatus Status = BookingStatus.Tentative,
    bool ManualPriceOverride = false,
    string? InternalNotes = null,
    string? GuestVisibleNotes = null,
    decimal SuggestedNightlyRate = 0m,
    bool OverrideCommercialRules = false,
    bool CreatedFromQuote = false,
    int? SavedQuoteId = null,
    decimal? ExpectedDepositAmount = null,
    DateOnly? DepositDueDate = null,
    string? CancellationPolicyApplied = null,
    string? Tags = null,
    bool ConfirmManualPriceChange = false,
    bool ConfirmReduceStayWithPayments = false
) : IRequest<AppResult<int>>;

public sealed class UpsertBookingCommandValidator : AbstractValidator<UpsertBookingCommand>
{
    public UpsertBookingCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.GuestId).GreaterThan(0);
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.Adults).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Children).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpsertBookingCommandHandler :
    IRequestHandler<UpsertBookingCommand, AppResult<int>>,
    IRequestHandler<CheckInOutCommand, AppResult>,
    IRequestHandler<DuplicateBookingCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public UpsertBookingCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<int>> Handle(UpsertBookingCommand request, CancellationToken ct)
    {
        var property = await _db.Properties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && p.IsActive, ct);
        if (property is null) return AppResult<int>.Fail("forbidden", "Hospedaje inválido, inactivo o sin acceso.");

        var unit = await _db.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UnitId && u.PropertyId == request.PropertyId && u.IsActive, ct);
        if (unit is null) return AppResult<int>.Fail("forbidden", "Unidad inválida o sin acceso.");
        if (unit.OperationalStatus == UnitOperationalStatus.Maintenance) return AppResult<int>.Fail("unit_unavailable", "La unidad está fuera de servicio.");

        var guestOk = await _db.Guests.AsNoTracking().AnyAsync(g => g.Id == request.GuestId && g.PropertyId == request.PropertyId && g.IsActive, ct);
        if (!guestOk) return AppResult<int>.Fail("forbidden", "Huésped inválido o sin acceso.");

        var blockedOverlap = await _db.BlockedDates.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId)
            .AnyAsync(x => x.DateFrom < request.CheckOutDate && request.CheckInDate < x.DateTo, ct);
        if (blockedOverlap) return AppResult<int>.Fail("overlap", "Las fechas seleccionadas están bloqueadas.");

        var hasOverlapBooking = await _db.Bookings.AsNoTracking().Where(b => b.PropertyId == request.PropertyId && b.UnitId == request.UnitId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => request.BookingId == null || b.Id != request.BookingId.Value)
            .AnyAsync(b => b.CheckInDate < request.CheckOutDate && request.CheckInDate < b.CheckOutDate, ct);
        if (hasOverlapBooking) return AppResult<int>.Fail("overlap", "La unidad ya tiene una reserva en ese rango.");

        var hasExternalOverlap = await _db.ExternalCalendarEvents.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId && !x.IsCancelled)
            .Where(x => x.ExternalChannelConnection.IsActive)
            .AnyAsync(x => x.StartDate < request.CheckOutDate && request.CheckInDate < x.EndDate, ct);
        if (hasExternalOverlap) return AppResult<int>.Fail("overlap_external", "La unidad tiene una reserva externa sincronizada en ese rango.");

        var promos = await _db.Promotions.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.UnitId == null || x.UnitId == request.UnitId) && x.IsActive).ToListAsync(ct);
        if (!request.OverrideCommercialRules)
        {
            var errors = CommercialPricing.ValidatePromotionsAndRules(promos, request.CheckInDate, request.CheckOutDate);
            if (errors.Count > 0) return AppResult<int>.Fail("rules", string.Join(" | ", errors));
        }

        var pricing = await CommercialPricing.CalculateAsync(_db, request.PropertyId, request.UnitId, request.CheckInDate, request.CheckOutDate, request.Adults, request.Children, ct);
        var isNew = request.BookingId is null;
        Booking entity;
        int originalNights = 0;
        decimal originalPaidAmount = 0m;

        if (isNew)
        {
            entity = new Booking
            {
                PropertyId = request.PropertyId,
                UnitId = request.UnitId,
                GuestId = request.GuestId,
                Status = request.Status,
                Source = request.Source,
                CreatedAt = DateTime.UtcNow,
                BookingCode = $"RSV-{DateTime.UtcNow:yyyyMMddHHmmss}-{request.UnitId}"
            };
            _db.Bookings.Add(entity);
        }
        else
        {
            entity = await _db.Bookings.Include(x => x.Events).FirstOrDefaultAsync(b => b.Id == request.BookingId!.Value && b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Reserva no encontrada.");

            originalNights = Math.Max(0, entity.CheckOutDate.DayNumber - entity.CheckInDate.DayNumber);
            originalPaidAmount = await _db.Payments.AsNoTracking().Where(p => p.BookingId == entity.Id && p.Status == PaymentStatus.Paid).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var requestedNights = Math.Max(0, request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber);
            if (requestedNights < originalNights && originalPaidAmount > 0 && !request.ConfirmReduceStayWithPayments)
                return AppResult<int>.Fail("warn_reduce_with_payments", "La estadía se reduce y la reserva ya tiene pagos registrados. Confirmá la operación antes de guardar.");

            if (request.ManualPriceOverride && (entity.TotalAmount != request.TotalAmount || !entity.ManualPriceOverride) && !request.ConfirmManualPriceChange)
                return AppResult<int>.Fail("warn_manual_price", "Estás modificando el precio manualmente. Confirmá la operación antes de guardar.");

            entity.UnitId = request.UnitId;
            entity.GuestId = request.GuestId;
            entity.Source = request.Source;
            entity.Status = request.Status;
        }

        entity.CheckInDate = request.CheckInDate;
        entity.CheckOutDate = request.CheckOutDate;
        entity.Adults = request.Adults;
        entity.Children = request.Children;
        entity.BaseAmount = pricing.BaseAmount;
        entity.PromotionsAmount = pricing.PromotionsAmount;
        entity.TotalAmount = request.ManualPriceOverride ? request.TotalAmount : pricing.FinalAmount;
        entity.Notes = request.Notes?.Trim();
        entity.InternalNotes = request.InternalNotes?.Trim();
        entity.GuestVisibleNotes = request.GuestVisibleNotes?.Trim();
        entity.ManualPriceOverride = request.ManualPriceOverride;
        entity.SuggestedNightlyRate = request.ManualPriceOverride && request.SuggestedNightlyRate > 0 ? request.SuggestedNightlyRate : pricing.SuggestedNightlyRate;
        entity.ExpectedDepositAmount = request.ExpectedDepositAmount ?? pricing.SuggestedDepositAmount;
        entity.DepositDueDate = request.DepositDueDate;
        entity.CreatedFromQuote = request.CreatedFromQuote;
        entity.SavedQuoteId = request.SavedQuoteId;
        entity.CancellationPolicyApplied = request.CancellationPolicyApplied ?? property.CancellationPolicy ?? property.DepositPolicy;
        entity.Tags = request.Tags?.Trim();
        entity.AppliedPromotionNames = pricing.AppliedPromotionNames;
        entity.OperationalStatus = request.Status switch
        {
            BookingStatus.CheckedIn => BookingOperationalStatus.CheckedIn,
            BookingStatus.CheckedOut => BookingOperationalStatus.CheckedOut,
            _ => BookingOperationalStatus.PendingCheckIn
        };
        entity.UpdatedAt = DateTime.UtcNow;

        var unitToUpdate = await _db.Units.FirstAsync(x => x.Id == request.UnitId, ct);
        if (entity.Status == BookingStatus.CheckedIn) unitToUpdate.OperationalStatus = UnitOperationalStatus.Occupied;
        if (entity.Status == BookingStatus.CheckedOut) unitToUpdate.OperationalStatus = UnitOperationalStatus.PendingCleaning;

        await _db.SaveChangesAsync(ct);

        if (request.SavedQuoteId.HasValue)
        {
            var savedQuote = await _db.SavedQuotes.FirstOrDefaultAsync(x => x.Id == request.SavedQuoteId.Value && x.PropertyId == request.PropertyId, ct);
            if (savedQuote is not null)
            {
                savedQuote.CreatedBookingId = entity.Id;
                savedQuote.Status = GestAI.Domain.Enums.SavedQuoteStatus.Converted;
            }
        }

        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = entity.PropertyId,
            BookingId = entity.Id,
            EventType = isNew ? BookingEventType.Created : BookingEventType.Audit,
            Title = isNew ? "Reserva creada" : "Reserva actualizada",
            Detail = isNew
                ? $"Total {entity.TotalAmount:0.00}. Promos: {entity.AppliedPromotionNames}"
                : $"Fechas {entity.CheckInDate:yyyy-MM-dd} a {entity.CheckOutDate:yyyy-MM-dd}. Total {entity.TotalAmount:0.00}. Pagado {originalPaidAmount:0.00}",
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });

        _db.AuditLogs.Add(new AuditLog
        {
            AccountId = property.AccountId,
            PropertyId = entity.PropertyId,
            EntityName = nameof(Booking),
            EntityId = entity.Id,
            Action = isNew ? "create" : "update",
            Summary = $"Reserva {entity.BookingCode} guardada",
            UserId = _current.UserId,
            UserName = _current.Email
        });

        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(entity.Id);
    }

    public async Task<AppResult> Handle(CheckInOutCommand request, CancellationToken ct)
    {
        var booking = await _db.Bookings.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == request.BookingId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (booking is null) return AppResult.Fail("not_found", "Reserva no encontrada.");

        if (request.IsCheckIn)
        {
            booking.Status = BookingStatus.CheckedIn;
            booking.OperationalStatus = BookingOperationalStatus.CheckedIn;
            booking.ActualCheckInTime = request.ActualTime ?? TimeOnly.FromDateTime(DateTime.Now);
            booking.CheckInNotes = request.Notes?.Trim();
            booking.DocumentationVerified = request.DocumentationVerified;
            booking.DepositVerified = request.DepositVerified;
            booking.FinalGuestsCount = request.FinalGuestsCount ?? Math.Max(booking.Adults + booking.Children, booking.FinalGuestsCount);
            booking.Unit.OperationalStatus = UnitOperationalStatus.Occupied;
        }
        else
        {
            booking.Status = BookingStatus.CheckedOut;
            booking.OperationalStatus = BookingOperationalStatus.CheckedOut;
            booking.ActualCheckOutTime = request.ActualTime ?? TimeOnly.FromDateTime(DateTime.Now);
            booking.CheckOutNotes = request.Notes?.Trim();
            booking.Unit.OperationalStatus = UnitOperationalStatus.PendingCleaning;

            var hasPendingCleaningTask = await _db.OperationalTasks.AnyAsync(t => t.PropertyId == request.PropertyId && t.BookingId == booking.Id && t.Type == OperationalTaskType.Cleaning && t.Status != OperationalTaskStatus.Completed && t.Status != OperationalTaskStatus.Cancelled, ct);
            if (!hasPendingCleaningTask)
            {
                _db.OperationalTasks.Add(new OperationalTask
                {
                    PropertyId = request.PropertyId,
                    UnitId = booking.UnitId,
                    BookingId = booking.Id,
                    Type = OperationalTaskType.Cleaning,
                    Priority = OperationalTaskPriority.High,
                    ScheduledDate = DateOnly.FromDateTime(DateTime.Today),
                    Title = $"Limpieza post checkout - {booking.Unit.Name}",
                    Notes = $"Generada automáticamente por checkout de {booking.BookingCode}."
                });
            }
        }

        booking.UpdatedAt = DateTime.UtcNow;
        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = booking.PropertyId,
            BookingId = booking.Id,
            EventType = request.IsCheckIn ? BookingEventType.CheckIn : BookingEventType.CheckOut,
            Title = request.IsCheckIn ? "Check-in realizado" : "Check-out realizado",
            Detail = request.Notes,
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });

        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }

    public async Task<AppResult<int>> Handle(DuplicateBookingCommand request, CancellationToken ct)
    {
        var source = await _db.Bookings.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SourceBookingId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        if (source is null)
            return AppResult<int>.Fail("not_found", "Reserva origen no encontrada.");

        var saveResult = await Handle(new UpsertBookingCommand(
            request.PropertyId,
            null,
            request.UnitId ?? source.UnitId,
            request.GuestId ?? source.GuestId,
            request.CheckInDate,
            request.CheckOutDate,
            request.Adults ?? source.Adults,
            request.Children ?? source.Children,
            source.TotalAmount,
            request.Notes ?? source.Notes,
            source.Source,
            BookingStatus.Tentative,
            source.ManualPriceOverride,
            source.InternalNotes,
            source.GuestVisibleNotes,
            source.SuggestedNightlyRate,
            false,
            false,
            null,
            source.ExpectedDepositAmount,
            source.DepositDueDate,
            source.CancellationPolicyApplied,
            source.Tags,
            source.ManualPriceOverride,
            true), ct);

        if (!saveResult.Success || saveResult.Data <= 0)
            return saveResult;

        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = request.PropertyId,
            BookingId = saveResult.Data,
            EventType = BookingEventType.Audit,
            Title = "Reserva duplicada",
            Detail = $"Duplicada desde la reserva #{source.Id} ({source.BookingCode}).",
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });
        await _db.SaveChangesAsync(ct);
        return saveResult;
    }
}
