using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Dashboard;

public sealed record GetDashboardSummaryQuery(int PropertyId, DateOnly? Today = null) : IRequest<AppResult<DashboardSummaryDto>>;

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, AppResult<DashboardSummaryDto>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public GetDashboardSummaryQueryHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<DashboardSummaryDto>> Handle(GetDashboardSummaryQuery request, CancellationToken ct)
    {
        var today = request.Today ?? DateOnly.FromDateTime(DateTime.Today);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var nextMonth = monthStart.AddMonths(1);
        var unitsCount = await _db.Units.AsNoTracking().CountAsync(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.IsActive, ct);
        var monthBookings = await _db.Bookings.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.Status != BookingStatus.Cancelled)
            .Where(x => x.CheckInDate < nextMonth && monthStart < x.CheckOutDate).ToListAsync(ct);
        var occupiedNights = monthBookings.Sum(x => Math.Max(0, Math.Min(x.CheckOutDate.DayNumber, nextMonth.DayNumber) - Math.Max(x.CheckInDate.DayNumber, monthStart.DayNumber)));
        var totalNights = unitsCount * (nextMonth.DayNumber - monthStart.DayNumber);
        var monthPayments = await _db.Payments.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.Status == PaymentStatus.Paid && x.Date >= monthStart && x.Date < nextMonth).SumAsync(x => (decimal?)x.Amount, ct) ?? 0m;
        var pendingBalance = await _db.Bookings.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.Status != BookingStatus.Cancelled)
            .Select(x => x.TotalAmount - (x.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m)).SumAsync(ct);
        var checkInsToday = await _db.Bookings.AsNoTracking().CountAsync(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.CheckInDate == today && x.Status != BookingStatus.Cancelled, ct);
        var checkOutsToday = await _db.Bookings.AsNoTracking().CountAsync(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.CheckOutDate == today && x.Status != BookingStatus.Cancelled, ct);
        var byStatusRaw = await _db.Bookings.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive))).GroupBy(x => x.Status).Select(g => new { Status = g.Key.ToString(), Count = g.Count() }).ToListAsync(ct);
        var upcomingRaw = await _db.Bookings.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.CheckInDate >= today && x.Status != BookingStatus.Cancelled)
            .OrderBy(x => x.CheckInDate).Take(8)
            .Select(x => new { x.Id, x.BookingCode, GuestName = x.Guest.FullName, UnitName = x.Unit.Name, x.CheckInDate, x.CheckOutDate, Pending = x.TotalAmount - (x.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m) }).ToListAsync(ct);
        var incomeSeries = new List<DashboardMonthPointDto>();
        var occSeries = new List<DashboardMonthPointDto>();
        for (var i = 5; i >= 0; i--)
        {
            var ms = monthStart.AddMonths(-i);
            var me = ms.AddMonths(1);
            var income = await _db.Payments.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.Status == PaymentStatus.Paid && x.Date >= ms && x.Date < me).SumAsync(x => (decimal?)x.Amount, ct) ?? 0m;
            var occBookings = await _db.Bookings.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.Status != BookingStatus.Cancelled)
                .Where(x => x.CheckInDate < me && ms < x.CheckOutDate).ToListAsync(ct);
            var occNights = occBookings.Sum(x => Math.Max(0, Math.Min(x.CheckOutDate.DayNumber, me.DayNumber) - Math.Max(x.CheckInDate.DayNumber, ms.DayNumber)));
            var totNights = unitsCount * (me.DayNumber - ms.DayNumber);
            incomeSeries.Add(new DashboardMonthPointDto(ms.ToString("MMM yy"), income));
            occSeries.Add(new DashboardMonthPointDto(ms.ToString("MMM yy"), totNights == 0 ? 0 : Math.Round((decimal)occNights * 100m / totNights, 2)));
        }
        var dto = new DashboardSummaryDto(totalNights == 0 ? 0 : Math.Round((decimal)occupiedNights * 100m / totalNights, 2), monthPayments, checkInsToday, checkOutsToday, pendingBalance,
            byStatusRaw.Select(x => new DashboardBookingStateDto(x.Status, x.Count)).ToList(), incomeSeries, occSeries,
            upcomingRaw.Select(x => new DashboardUpcomingBookingDto(x.Id, x.BookingCode, x.GuestName, x.UnitName, x.CheckInDate, x.CheckOutDate, x.Pending)).ToList());
        return AppResult<DashboardSummaryDto>.Ok(dto);
    }
}
