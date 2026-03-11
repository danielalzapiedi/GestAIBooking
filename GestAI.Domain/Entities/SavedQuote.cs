using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class SavedQuote : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public string PublicToken { get; set; } = null!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int Adults { get; set; }
    public int Children { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal PromotionsAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SuggestedDepositAmount { get; set; }
    public string? Summary { get; set; }
    public string? AppliedPromotionNames { get; set; }
    public SavedQuoteStatus Status { get; set; } = SavedQuoteStatus.Saved;
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public string? GuestPhone { get; set; }
    public int? CreatedBookingId { get; set; }
    public Booking? CreatedBooking { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
