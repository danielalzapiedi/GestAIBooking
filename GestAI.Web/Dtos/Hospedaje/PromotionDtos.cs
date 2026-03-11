namespace GestAI.Web.Dtos;

public sealed record PromotionDto(int Id, int PropertyId, int? UnitId, string? UnitName, string Name, string? Description, bool IsActive, bool IsDeleted, DiscountValueType ValueType, PromotionScope Scope, decimal Value, bool IsCumulative, int Priority, DateOnly DateFrom, DateOnly DateTo, int? MinNights, int? MaxNights, int? BookingWindowDaysMin, int? BookingWindowDaysMax, string? AllowedCheckInDays, string? AllowedCheckOutDays);
public sealed record UpsertPromotionCommand(int PropertyId, int? PromotionId, int? UnitId, string Name, string? Description, bool IsActive, DiscountValueType ValueType, PromotionScope Scope, decimal Value, bool IsCumulative, int Priority, DateOnly DateFrom, DateOnly DateTo, int? MinNights, int? MaxNights, int? BookingWindowDaysMin, int? BookingWindowDaysMax, string? AllowedCheckInDays, string? AllowedCheckOutDays);
public sealed record TogglePromotionStatusCommand(int PropertyId, int PromotionId, bool IsActive);
