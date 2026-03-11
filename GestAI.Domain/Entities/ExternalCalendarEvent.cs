using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class ExternalCalendarEvent : Entity
{
    public int ExternalChannelConnectionId { get; set; }
    public ExternalChannelConnection ExternalChannelConnection { get; set; } = null!;
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public string ExternalEventUid { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Summary { get; set; }
    public ExternalChannelType SourceChannel { get; set; } = ExternalChannelType.Other;
    public string? RawData { get; set; }
    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsCancelled { get; set; } = false;
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
    public string? SyncHash { get; set; }
}
