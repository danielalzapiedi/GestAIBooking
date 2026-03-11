using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Bookings;

public sealed record CancelBookingCommand(int PropertyId, int BookingId, string? Reason = null) : IRequest<AppResult>;

public sealed class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.BookingId).GreaterThan(0);
    }
}

public sealed class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public CancelBookingCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.PropertyId == request.PropertyId && b.Id == request.BookingId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (booking is null) return AppResult.Fail("not_found", "Reserva no encontrada.");

        var paid = await _db.Payments.AsNoTracking().Where(p => p.BookingId == booking.Id && p.Status == PaymentStatus.Paid).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = string.IsNullOrWhiteSpace(request.Reason) ? (paid > 0 ? $"Reserva cancelada con pagos registrados ({paid:0.00})." : null) : request.Reason.Trim();
        booking.UpdatedAt = DateTime.UtcNow;

        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = booking.PropertyId,
            BookingId = booking.Id,
            EventType = BookingEventType.Cancelled,
            Title = "Reserva cancelada",
            Detail = booking.CancellationReason,
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
