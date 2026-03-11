using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class ExternalSyncLog : Entity
{
    public int ExternalChannelConnectionId { get; set; }
    public ExternalChannelConnection ExternalChannelConnection { get; set; } = null!;
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public ExternalSyncStatus Status { get; set; } = ExternalSyncStatus.Never;
    public int ProcessedEvents { get; set; }
    public int ImportedEvents { get; set; }
    public int UpdatedEvents { get; set; }
    public int CancelledEvents { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
