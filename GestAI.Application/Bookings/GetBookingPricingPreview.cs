using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Application.Common.Pricing;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed class GetBookingPricingPreviewQueryValidator : AbstractValidator<GetBookingPricingPreviewQuery>
{
    public GetBookingPricingPreviewQueryValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.CheckOutDate).GreaterThan(x => x.CheckInDate);
        RuleFor(x => x.Adults).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Children).GreaterThanOrEqualTo(0);
    }
}

public sealed class GetBookingPricingPreviewQueryHandler : IRequestHandler<GetBookingPricingPreviewQuery, AppResult<BookingPricingPreviewDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetBookingPricingPreviewQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<BookingPricingPreviewDto>> Handle(GetBookingPricingPreviewQuery request, CancellationToken ct)
    {
        var unit = await _db.Units.AsNoTracking()
            .Where(x => x.Id == request.UnitId
                && x.PropertyId == request.PropertyId
                && x.IsActive
                && x.Property.IsActive
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(x => new { x.Id, x.Name, x.OperationalStatus })
            .FirstOrDefaultAsync(ct);
        if (unit is null)
            return AppResult<BookingPricingPreviewDto>.Fail("not_found", "Unidad no encontrada.");

        var pricing = await CommercialPricing.CalculateAsync(_db, request.PropertyId, request.UnitId, request.CheckInDate, request.CheckOutDate, request.Adults, request.Children, ct);
        var validations = pricing.Rules.ToList();
        if (unit.OperationalStatus == UnitOperationalStatus.Maintenance)
            validations.Add("La unidad está fuera de servicio por mantenimiento.");

        return AppResult<BookingPricingPreviewDto>.Ok(new BookingPricingPreviewDto(
            request.PropertyId,
            request.UnitId,
            unit.Name,
            request.CheckInDate,
            request.CheckOutDate,
            request.Adults,
            request.Children,
            pricing.BaseAmount,
            pricing.PromotionsAmount,
            pricing.FinalAmount,
            pricing.SuggestedNightlyRate,
            pricing.SuggestedDepositAmount,
            validations.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            pricing.Lines));
    }
}
