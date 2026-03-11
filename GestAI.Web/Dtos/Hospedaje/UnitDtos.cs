namespace GestAI.Web.Dtos;

public sealed class UnitListItemDto
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CapacityAdults { get; set; }
    public int CapacityChildren { get; set; }
    public bool IsActive { get; set; }
    public decimal BaseRate { get; set; }
    public int TotalCapacity { get; set; }
    public string? ShortDescription { get; set; }
    public int DisplayOrder { get; set; }
    public UnitOperationalStatus OperationalStatus { get; set; }
}

public sealed record UpsertUnitCommand(int PropertyId, int? UnitId, string Name, int CapacityAdults, int CapacityChildren, decimal BaseRate, int TotalCapacity, string? ShortDescription, bool IsActive, int DisplayOrder, UnitOperationalStatus OperationalStatus);
