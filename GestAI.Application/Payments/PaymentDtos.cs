using GestAI.Domain.Enums;

namespace GestAI.Application.Payments;

public sealed record PaymentDto(int Id, int BookingId, decimal Amount, PaymentMethod Method, DateOnly Date, PaymentStatus Status, string? Notes);
