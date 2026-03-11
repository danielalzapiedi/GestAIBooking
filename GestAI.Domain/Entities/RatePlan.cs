using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class RatePlan : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int UnitId { get; set; }
    public Unit Unit { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal BaseNightlyRate { get; set; } = 0m;
    public bool WeekendAdjustmentEnabled { get; set; }
    public RateAdjustmentType WeekendAdjustmentType { get; set; } = RateAdjustmentType.Fixed;
    public decimal WeekendAdjustmentValue { get; set; } = 0m;
    public bool IsActive { get; set; } = true;

    public ICollection<SeasonalRate> SeasonalRates { get; set; } = new List<SeasonalRate>();
    public ICollection<DateRangeRate> DateRangeRates { get; set; } = new List<DateRangeRate>();
}
