namespace GestAI.Web.Dtos;

public sealed record PaymentDto(int Id, int BookingId, decimal Amount, PaymentMethod Method, DateOnly Date, PaymentStatus Status, string? Notes);

public sealed record CreatePaymentCommand(int PropertyId, int BookingId, decimal Amount, PaymentMethod Method, DateOnly Date, PaymentStatus Status, string? Notes);
