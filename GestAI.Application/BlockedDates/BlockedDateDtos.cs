namespace GestAI.Application.BlockedDates;

public sealed record BlockedDateDto(int Id, int PropertyId, int UnitId, DateOnly DateFrom, DateOnly DateTo, string? Reason);
