using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class Booking : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public int GuestId { get; set; }
    public Guest Guest { get; set; } = null!;
    public string BookingCode { get; set; } = null!;
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Tentative;
    public BookingSource Source { get; set; } = BookingSource.Direct;
    public BookingOperationalStatus OperationalStatus { get; set; } = BookingOperationalStatus.PendingCheckIn;
    public int Adults { get; set; } = 1;
    public int Children { get; set; } = 0;
    public int FinalGuestsCount { get; set; } = 0;
    public decimal TotalAmount { get; set; } = 0m;
    public decimal SuggestedNightlyRate { get; set; } = 0m;
    public decimal BaseAmount { get; set; } = 0m;
    public decimal PromotionsAmount { get; set; } = 0m;
    public decimal ExpectedDepositAmount { get; set; } = 0m;
    public DateOnly? DepositDueDate { get; set; }
    public bool DepositVerified { get; set; } = false;
    public bool DocumentationVerified { get; set; } = false;
    public bool ManualPriceOverride { get; set; } = false;
    public bool CreatedFromQuote { get; set; } = false;
    public int? SavedQuoteId { get; set; }
    public SavedQuote? SavedQuote { get; set; }
    public string? AppliedPromotionNames { get; set; }
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? GuestVisibleNotes { get; set; }
    public string? CancellationPolicyApplied { get; set; }
    public string? CancellationReason { get; set; }
    public string? Tags { get; set; }
    public TimeOnly? ActualCheckInTime { get; set; }
    public TimeOnly? ActualCheckOutTime { get; set; }
    public string? CheckInNotes { get; set; }
    public string? CheckOutNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<BookingEvent> Events { get; set; } = new List<BookingEvent>();
}
