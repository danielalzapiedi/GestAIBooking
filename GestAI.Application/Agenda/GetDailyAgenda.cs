using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Agenda;

public sealed record GetDailyAgendaQuery(int PropertyId, DateOnly Date) : IRequest<AppResult<DailyAgendaDto>>;

public sealed class GetDailyAgendaQueryHandler : IRequestHandler<GetDailyAgendaQuery, AppResult<DailyAgendaDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetDailyAgendaQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<DailyAgendaDto>> Handle(GetDailyAgendaQuery request, CancellationToken ct)
    {
        var from = request.Date;
        var to7 = request.Date.AddDays(7);

        var baseQ = _db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(b => b.Status != BookingStatus.Cancelled);

        var checkins = await baseQ.Where(b => b.CheckInDate == from)
            .OrderBy(b => b.Unit.Name)
            .Select(b => new AgendaBookingDto(b.Id, b.UnitId, b.Unit.Name, b.GuestId, b.Guest.FullName, b.CheckInDate, b.CheckOutDate, b.Status))
            .ToListAsync(ct);

        var checkouts = await baseQ.Where(b => b.CheckOutDate == from)
            .OrderBy(b => b.Unit.Name)
            .Select(b => new AgendaBookingDto(b.Id, b.UnitId, b.Unit.Name, b.GuestId, b.Guest.FullName, b.CheckInDate, b.CheckOutDate, b.Status))
            .ToListAsync(ct);

        var next = await baseQ.Where(b => b.CheckInDate >= from && b.CheckInDate < to7)
            .OrderBy(b => b.CheckInDate).ThenBy(b => b.Unit.Name)
            .Select(b => new AgendaBookingDto(b.Id, b.UnitId, b.Unit.Name, b.GuestId, b.Guest.FullName, b.CheckInDate, b.CheckOutDate, b.Status))
            .ToListAsync(ct);

        return AppResult<DailyAgendaDto>.Ok(new DailyAgendaDto(request.Date, checkins, checkouts, next));
    }
}
