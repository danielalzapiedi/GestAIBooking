namespace GestAI.Web.Dtos;

public sealed record OperationalTaskDto(int Id, int PropertyId, int? UnitId, string? UnitName, int? BookingId, string? BookingCode, OperationalTaskType Type, OperationalTaskStatus Status, OperationalTaskPriority Priority, DateOnly ScheduledDate, string Title, string? Notes, DateTime CreatedAtUtc, DateTime? CompletedAtUtc);
