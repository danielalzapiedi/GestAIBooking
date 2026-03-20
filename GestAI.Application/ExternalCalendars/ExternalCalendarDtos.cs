using GestAI.Domain.Enums;

namespace GestAI.Application.ExternalCalendars;

public sealed record ExternalCalendarConnectionDto(
    int Id,
    int PropertyId,
    int UnitId,
    string UnitName,
    ExternalChannelType ChannelType,
    string DisplayName,
    string ImportCalendarUrl,
    string ExportUrl,
    bool IsActive,
    bool AutoSyncEnabled,
    DateTime? LastSyncAt,
    ExternalSyncStatus LastSyncStatus,
    string? LastSyncMessage);

public sealed record ExternalCalendarEventDto(
    int Id,
    int ExternalChannelConnectionId,
    int UnitId,
    DateOnly StartDate,
    DateOnly EndDate,
    string? Summary,
    ExternalChannelType SourceChannel,
    bool IsCancelled);

public sealed record ExternalSyncLogDto(
    int Id,
    int ExternalChannelConnectionId,
    ExternalSyncStatus Status,
    int ProcessedEvents,
    int ImportedEvents,
    int UpdatedEvents,
    int CancelledEvents,
    string? Message,
    DateTime CreatedAtUtc);

public sealed record ExternalCalendarSyncConfigurationDto(
    bool AutoSyncWorkerEnabled,
    int IntervalMinutes,
    int BatchSize);
