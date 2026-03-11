using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class DateRangeRate : Entity
{
    public int RatePlanId { get; set; }
    public RatePlan RatePlan { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public RateAdjustmentType AdjustmentType { get; set; } = RateAdjustmentType.Fixed;
    public decimal AdjustmentValue { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
}
