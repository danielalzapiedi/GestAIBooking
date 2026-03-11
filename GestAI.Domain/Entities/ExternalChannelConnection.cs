using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class ExternalChannelConnection : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public ExternalChannelType ChannelType { get; set; } = ExternalChannelType.Other;
    public string DisplayName { get; set; } = null!;
    public string ImportCalendarUrl { get; set; } = null!;
    public string ExportToken { get; set; } = Guid.NewGuid().ToString("N");
    public bool IsActive { get; set; } = true;
    public bool AutoSyncEnabled { get; set; } = false;
    public DateTime? LastSyncAt { get; set; }
    public ExternalSyncStatus LastSyncStatus { get; set; } = ExternalSyncStatus.Never;
    public string? LastSyncMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<ExternalCalendarEvent> Events { get; set; } = new List<ExternalCalendarEvent>();
    public ICollection<ExternalSyncLog> SyncLogs { get; set; } = new List<ExternalSyncLog>();
}
