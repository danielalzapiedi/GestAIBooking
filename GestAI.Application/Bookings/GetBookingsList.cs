using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record GetBookingsListQuery(int PropertyId, DateOnly From, DateOnly To) : IRequest<AppResult<List<BookingListItemDto>>>;

public sealed class GetBookingsListQueryHandler : IRequestHandler<GetBookingsListQuery, AppResult<List<BookingListItemDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetBookingsListQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<BookingListItemDto>>> Handle(GetBookingsListQuery request, CancellationToken ct)
    {
        var items = await _db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(b => b.CheckInDate < request.To && request.From < b.CheckOutDate)
            .OrderByDescending(b => b.CheckInDate)
            .ThenBy(b => b.Unit.Name)
            .Select(b => new BookingListItemDto(
                b.Id,
                b.PropertyId,
                b.UnitId,
                b.Unit.Name,
                b.GuestId,
                b.Guest.FullName,
                b.BookingCode,
                b.CheckInDate,
                b.CheckOutDate,
                b.Adults,
                b.Children,
                b.Status,
                b.Source,
                b.OperationalStatus,
                b.TotalAmount,
                b.TotalAmount - (b.Payments.Where(p => p.Status == PaymentStatus.Paid).Select(p => (decimal?)p.Amount).Sum() ?? 0m),
                b.CheckOutDate.DayNumber - b.CheckInDate.DayNumber,
                b.ExpectedDepositAmount,
                b.CreatedFromQuote,
                b.Tags))
            .ToListAsync(ct);

        return AppResult<List<BookingListItemDto>>.Ok(items);
    }
}
