using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Payments;

public sealed record DeletePaymentCommand(int PropertyId, int PaymentId) : IRequest<AppResult>;

public sealed class DeletePaymentCommandValidator : AbstractValidator<DeletePaymentCommand>
{
    public DeletePaymentCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.PaymentId).GreaterThan(0);
    }
}

public sealed class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IPropertyFeatureService _features;

    public DeletePaymentCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _features = features;
    }

    public async Task<AppResult> Handle(DeletePaymentCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Payments, ct))
            return AppResult.Fail("feature_disabled", "La gestión de pagos está desactivada para este hospedaje.");

        var payment = await _db.Payments
            .FirstOrDefaultAsync(p => p.PropertyId == request.PropertyId && p.Id == request.PaymentId && p.Property.Account.OwnerUserId == _current.UserId, ct);

        if (payment is null)
            return AppResult.Fail("not_found", "Pago no encontrado.");

        _db.Payments.Remove(payment);
        _db.BookingEvents.Add(new BookingEvent
        {
            PropertyId = payment.PropertyId,
            BookingId = payment.BookingId,
            EventType = BookingEventType.Audit,
            Title = "Pago eliminado",
            Detail = $"Pago #{payment.Id} eliminado. Monto {payment.Amount:0.00}.",
            ChangedByUserId = _current.UserId,
            ChangedByName = _current.Email
        });
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
