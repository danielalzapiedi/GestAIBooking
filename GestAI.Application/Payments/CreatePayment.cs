using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Payments;

public sealed record CreatePaymentCommand(
    int PropertyId,
    int BookingId,
    decimal Amount,
    PaymentMethod Method,
    DateOnly Date,
    PaymentStatus Status,
    string? Notes
) : IRequest<AppResult<int>>;

public sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.BookingId).GreaterThan(0);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IPropertyFeatureService _features;

    public CreatePaymentCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _features = features;
    }

    public async Task<AppResult<int>> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Payments, ct))
            return AppResult<int>.Fail("feature_disabled", "La gestión de pagos está desactivada para este hospedaje.");

        var booking = await _db.Bookings
            .FirstOrDefaultAsync(b => b.Id == request.BookingId && b.PropertyId == request.PropertyId && (b.Property.Account.OwnerUserId == _current.UserId || b.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        if (booking is null)
            return AppResult<int>.Fail("not_found", "Reserva no encontrada.");

        var entity = new Payment
        {
            PropertyId = request.PropertyId,
            BookingId = request.BookingId,
            Amount = request.Amount,
            Method = request.Method,
            Date = request.Date,
            Status = request.Status,
            Notes = request.Notes?.Trim()
        };

        _db.Payments.Add(entity);
        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = request.PropertyId,
            BookingId = request.BookingId,
            EventType = BookingEventType.PaymentRegistered,
            Title = "Pago registrado",
            Detail = $"{request.Amount:0.00} - {request.Method} - {request.Status}",
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });
        await _db.SaveChangesAsync(ct);

        return AppResult<int>.Ok(entity.Id);
    }
}
