using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Reports;

public sealed record GetReportsQuery(int PropertyId, DateOnly From, DateOnly ToExclusive, int Year, int Month) : IRequest<AppResult<ReportsDto>>;

public sealed class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, AppResult<ReportsDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;
    private readonly IUserAccessService _access;

    public GetReportsQueryHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features, IUserAccessService access)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
        _access = access;
    }

    public async Task<AppResult<ReportsDto>> Handle(GetReportsQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Reports, ct))
            return AppResult<ReportsDto>.Fail("feature_disabled", "Los reportes están desactivados para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Reports, ct))
            return AppResult<ReportsDto>.Fail("forbidden", "No tenés acceso al módulo de reportes.");

        var propertyAccess = _db.Properties.AsNoTracking()
            .Where(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)));

        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Reports, ct))
            return AppResult<ReportsDto>.Fail("forbidden", "No tenés permisos para usar el módulo de reportes.");

        var units = await _db.Units.AsNoTracking()
            .Where(u => u.PropertyId == request.PropertyId && u.IsActive)
            .Select(u => new { u.Id, u.Name })
            .ToListAsync(ct);

        var paidIncome = await _db.Payments.AsNoTracking()
            .Where(p => p.PropertyId == request.PropertyId && (p.Property.Account.OwnerUserId == _current.UserId || p.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(p => p.Status == PaymentStatus.Paid)
            .Where(p => p.Date.Year == request.Year && p.Date.Month == request.Month)
            .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

        var periodBookings = await _db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(b => b.CheckInDate < request.ToExclusive && request.From < b.CheckOutDate)
            .Select(b => new
            {
                b.Id,
                b.UnitId,
                UnitName = b.Unit.Name,
                b.Status,
                b.CheckInDate,
                b.CheckOutDate,
                b.TotalAmount,
                b.ExpectedDepositAmount,
                PaidAmount = b.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m
            })
            .ToListAsync(ct);

        var unitsCount = units.Count;
        var totalDays = Math.Max(0, request.ToExclusive.DayNumber - request.From.DayNumber);
        var totalNightsAvailable = Math.Max(0, unitsCount * totalDays);

        var nonCancelled = periodBookings.Where(b => b.Status != BookingStatus.Cancelled).ToList();
        var nightsOccupied = 0;
        decimal totalStayNights = 0;
        foreach (var b in nonCancelled)
        {
            var start = b.CheckInDate < request.From ? request.From : b.CheckInDate;
            var end = b.CheckOutDate > request.ToExclusive ? request.ToExclusive : b.CheckOutDate;
            var nights = Math.Max(0, end.DayNumber - start.DayNumber);
            nightsOccupied += nights;
            totalStayNights += Math.Max(0, b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber);
        }

        var avgStay = nonCancelled.Count == 0 ? 0m : Math.Round(totalStayNights / nonCancelled.Count, 2);
        var occPct = totalNightsAvailable == 0 ? 0m : Math.Round((decimal)nightsOccupied / totalNightsAvailable * 100m, 2);
        var avgIncomePerNight = nightsOccupied == 0 ? 0m : Math.Round(paidIncome / nightsOccupied, 2);

        var depositsCollected = nonCancelled.Sum(b => Math.Min(b.ExpectedDepositAmount, b.PaidAmount));
        var depositsPending = nonCancelled.Sum(b => Math.Max(0m, b.ExpectedDepositAmount - b.PaidAmount));

        var occupancy = new OccupancyReportDto(request.From, request.ToExclusive, totalNightsAvailable, nightsOccupied, occPct, avgStay);
        var income = new IncomeReportDto(request.Year, request.Month, paidIncome, depositsCollected, depositsPending, avgIncomePerNight);

        var byStatus = periodBookings
            .GroupBy(x => x.Status)
            .OrderByDescending(g => g.Count())
            .Select(g => new ReportCountItemDto(g.Key.ToString(), g.Count()))
            .ToList();

        var totalBookings = periodBookings.Count;
        var unitItems = new List<UnitOccupancyItemDto>();
        foreach (var unit in units)
        {
            var unitBookings = nonCancelled.Where(x => x.UnitId == unit.Id).ToList();
            var unitNights = 0;
            foreach (var b in unitBookings)
            {
                var start = b.CheckInDate < request.From ? request.From : b.CheckInDate;
                var end = b.CheckOutDate > request.ToExclusive ? request.ToExclusive : b.CheckOutDate;
                unitNights += Math.Max(0, end.DayNumber - start.DayNumber);
            }

            var unitIncome = unitBookings.Sum(x => x.PaidAmount);
            unitItems.Add(new UnitOccupancyItemDto(
                unit.Id,
                unit.Name,
                unitNights,
                totalDays,
                totalDays == 0 ? 0m : Math.Round((decimal)unitNights / totalDays * 100m, 2),
                unitIncome,
                unitNights == 0 ? 0m : Math.Round(unitIncome / unitNights, 2)));
        }

        return AppResult<ReportsDto>.Ok(new ReportsDto(income, occupancy, totalBookings, byStatus, unitItems.OrderByDescending(x => x.OccupancyPercent).ToList()));
    }
}
