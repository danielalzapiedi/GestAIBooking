namespace GestAI.Web.Dtos;

public enum ExternalChannelType
{
    Booking = 0,
    Airbnb = 1,
    Other = 99
}

public enum ExternalSyncStatus
{
    Never = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}

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

public sealed record UpsertExternalCalendarConnectionCommand(
    int PropertyId,
    int? ConnectionId,
    int UnitId,
    ExternalChannelType ChannelType,
    string DisplayName,
    string ImportCalendarUrl,
    bool IsActive,
    bool AutoSyncEnabled);

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
