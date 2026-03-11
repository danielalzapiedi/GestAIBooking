using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class Payment : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public PaymentStatus Status { get; set; } = PaymentStatus.Paid;

    public string? Notes { get; set; }
}
