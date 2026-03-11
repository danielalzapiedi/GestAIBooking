using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class SeasonalRate : Entity
{
    public int RatePlanId { get; set; }
    public RatePlan RatePlan { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int StartMonth { get; set; }
    public int StartDay { get; set; }
    public int EndMonth { get; set; }
    public int EndDay { get; set; }
    public RateAdjustmentType AdjustmentType { get; set; } = RateAdjustmentType.Fixed;
    public decimal AdjustmentValue { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
}
