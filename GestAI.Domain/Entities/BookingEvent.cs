using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class BookingEvent : Entity
{
    public int PropertyId { get; set; }
    public int BookingId { get; set; }
    public Property Property { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
    public BookingEventType EventType { get; set; }
    public string Title { get; set; } = null!;
    public string? Detail { get; set; }
    public string? ChangedByUserId { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
}
