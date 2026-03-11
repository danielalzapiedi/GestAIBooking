using GestAI.Domain.Common;
using GestAI.Domain.Enums;

namespace GestAI.Domain.Entities;

public sealed class Promotion : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsCumulative { get; set; } = false;
    public int Priority { get; set; } = 100;
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public DiscountValueType ValueType { get; set; } = DiscountValueType.Percentage;
    public PromotionScope Scope { get; set; } = PromotionScope.EntireStay;
    public decimal Value { get; set; }
    public int? MinNights { get; set; }
    public int? MaxNights { get; set; }
    public int? BookingWindowDaysMin { get; set; }
    public int? BookingWindowDaysMax { get; set; }
    public string? AllowedCheckInDays { get; set; }
    public string? AllowedCheckOutDays { get; set; }
}
