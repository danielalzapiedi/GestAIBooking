using GestAI.Domain.Enums;

namespace GestAI.Application.Units;

public sealed record UnitListItemDto(
    int Id,
    int PropertyId,
    string Name,
    int CapacityAdults,
    int CapacityChildren,
    bool IsActive,
    decimal BaseRate,
    int TotalCapacity,
    string? ShortDescription,
    int DisplayOrder,
    UnitOperationalStatus OperationalStatus);
