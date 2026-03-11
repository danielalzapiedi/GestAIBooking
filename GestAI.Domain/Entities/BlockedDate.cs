using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class BlockedDate : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; } // exclusivo

    public string? Reason { get; set; }
}
