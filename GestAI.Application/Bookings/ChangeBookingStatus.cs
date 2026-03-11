using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record ChangeBookingStatusCommand(int PropertyId, int BookingId, BookingStatus Status, string? Reason = null) : IRequest<AppResult>;

public sealed class ChangeBookingStatusCommandHandler : IRequestHandler<ChangeBookingStatusCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public ChangeBookingStatusCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(ChangeBookingStatusCommand request, CancellationToken ct)
    {
        var booking = await _db.Bookings.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == request.BookingId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (booking is null) return AppResult.Fail("not_found", "Reserva no encontrada.");

        booking.Status = request.Status;
        booking.UpdatedAt = DateTime.UtcNow;
        booking.OperationalStatus = request.Status switch
        {
            BookingStatus.CheckedIn => BookingOperationalStatus.CheckedIn,
            BookingStatus.CheckedOut => BookingOperationalStatus.CheckedOut,
            _ => BookingOperationalStatus.PendingCheckIn
        };

        if (request.Status == BookingStatus.CheckedIn)
            booking.Unit.OperationalStatus = UnitOperationalStatus.Occupied;
        else if (request.Status == BookingStatus.CheckedOut)
            booking.Unit.OperationalStatus = UnitOperationalStatus.PendingCleaning;
        else if (request.Status == BookingStatus.Cancelled)
            booking.CancellationReason = request.Reason;

        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = booking.PropertyId,
            BookingId = booking.Id,
            EventType = request.Status == BookingStatus.Cancelled ? BookingEventType.Cancelled : BookingEventType.StatusChanged,
            Title = $"Estado actualizado a {request.Status}",
            Detail = request.Reason,
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });

        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
