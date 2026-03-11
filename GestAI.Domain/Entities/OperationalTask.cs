using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class OperationalTask : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
    public OperationalTaskType Type { get; set; }
    public OperationalTaskStatus Status { get; set; } = OperationalTaskStatus.Pending;
    public OperationalTaskPriority Priority { get; set; } = OperationalTaskPriority.Medium;
    public DateOnly ScheduledDate { get; set; }
    public string? ResponsibleName { get; set; }
    public string Title { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
