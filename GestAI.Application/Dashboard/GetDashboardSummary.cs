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
        var seriesStart = monthStart.AddMonths(-5);
        var unitsCount = await _db.Units.AsNoTracking().CountAsync(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && x.IsActive, ct);

        var bookingsWindow = await _db.Bookings.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive))
                && x.Status != BookingStatus.Cancelled
                && x.CheckInDate < nextMonth
                && seriesStart < x.CheckOutDate)
            .Select(x => new
            {
                x.Id,
                x.Status,
                x.BookingCode,
                x.CheckInDate,
                x.CheckOutDate,
                GuestName = x.Guest.FullName,
                UnitName = x.Unit.Name,
                Pending = x.TotalAmount - (x.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m)
            })
            .ToListAsync(ct);

        var occupiedNights = bookingsWindow.Sum(x => Math.Max(0, Math.Min(x.CheckOutDate.DayNumber, nextMonth.DayNumber) - Math.Max(x.CheckInDate.DayNumber, monthStart.DayNumber)));
        var totalNights = unitsCount * (nextMonth.DayNumber - monthStart.DayNumber);

        var paidPaymentsWindow = await _db.Payments.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive))
                && x.Status == PaymentStatus.Paid
                && x.Date >= seriesStart
                && x.Date < nextMonth)
            .Select(x => new { x.Date, x.Amount })
            .ToListAsync(ct);

        var monthPayments = paidPaymentsWindow.Where(x => x.Date >= monthStart && x.Date < nextMonth).Sum(x => x.Amount);
        var pendingBalance = await _db.Bookings.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive))
                && x.Status != BookingStatus.Cancelled)
            .Select(x => x.TotalAmount - (x.Payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => (decimal?)p.Amount) ?? 0m))
            .SumAsync(ct);

        var checkInsToday = bookingsWindow.Count(x => x.CheckInDate == today);
        var checkOutsToday = bookingsWindow.Count(x => x.CheckOutDate == today);

        var byStatusRaw = await _db.Bookings.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync(ct);

        var upcomingRaw = bookingsWindow
            .Where(x => x.CheckInDate >= today)
            .OrderBy(x => x.CheckInDate)
            .Take(8)
            .ToList();

        var incomeSeries = new List<DashboardMonthPointDto>();
        var occSeries = new List<DashboardMonthPointDto>();
        for (var i = 5; i >= 0; i--)
        {
            var ms = monthStart.AddMonths(-i);
            var me = ms.AddMonths(1);
            var income = paidPaymentsWindow.Where(x => x.Date >= ms && x.Date < me).Sum(x => x.Amount);
            var occBookings = bookingsWindow.Where(x => x.CheckInDate < me && ms < x.CheckOutDate).ToList();
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
