using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record GetBookingsByRangeQuery(int PropertyId, DateOnly From, DateOnly To) : IRequest<AppResult<List<CalendarBookingDto>>>;

public sealed class GetBookingsByRangeQueryHandler : IRequestHandler<GetBookingsByRangeQuery, AppResult<List<CalendarBookingDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetBookingsByRangeQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<CalendarBookingDto>>> Handle(GetBookingsByRangeQuery request, CancellationToken ct)
    {
        var items = await _db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(b => b.Status != BookingStatus.Cancelled)
            .Where(b => b.CheckInDate < request.To && request.From < b.CheckOutDate)
            .OrderBy(b => b.CheckInDate)
            .ThenBy(b => b.Unit.Name)
            .Select(b => new CalendarBookingDto(
                b.Id,
                b.UnitId,
                b.GuestId,
                b.Guest.FullName,
                b.BookingCode,
                b.CheckInDate,
                b.CheckOutDate,
                b.Status,
                b.Source,
                b.OperationalStatus,
                b.TotalAmount,
                b.TotalAmount - (b.Payments.Where(p => p.Status == PaymentStatus.Paid).Select(p => (decimal?)p.Amount).Sum() ?? 0m)))
            .ToListAsync(ct);

        return AppResult<List<CalendarBookingDto>>.Ok(items);
    }
}
