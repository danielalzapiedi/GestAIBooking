namespace GestAI.Web.Dtos;

public sealed record BlockedDateDto(int Id, int PropertyId, int UnitId, DateOnly DateFrom, DateOnly DateTo, string? Reason);

public sealed record CreateBlockedDateCommand(int PropertyId, int UnitId, DateOnly DateFrom, DateOnly DateTo, string? Reason);
