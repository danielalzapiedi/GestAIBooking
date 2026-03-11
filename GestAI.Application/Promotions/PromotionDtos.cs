using GestAI.Domain.Enums;

namespace GestAI.Application.Promotions;

public sealed record PromotionDto(
    int Id,
    int PropertyId,
    int? UnitId,
    string? UnitName,
    string Name,
    string? Description,
    bool IsActive,
    bool IsDeleted,
    DiscountValueType ValueType,
    PromotionScope Scope,
    decimal Value,
    bool IsCumulative,
    int Priority,
    DateOnly DateFrom,
    DateOnly DateTo,
    int? MinNights,
    int? MaxNights,
    int? BookingWindowDaysMin,
    int? BookingWindowDaysMax,
    string? AllowedCheckInDays,
    string? AllowedCheckOutDays);

public sealed record UpsertPromotionCommand(
    int PropertyId,
    int? PromotionId,
    int? UnitId,
    string Name,
    string? Description,
    bool IsActive,
    DiscountValueType ValueType,
    PromotionScope Scope,
    decimal Value,
    bool IsCumulative,
    int Priority,
    DateOnly DateFrom,
    DateOnly DateTo,
    int? MinNights,
    int? MaxNights,
    int? BookingWindowDaysMin,
    int? BookingWindowDaysMax,
    string? AllowedCheckInDays,
    string? AllowedCheckOutDays) : MediatR.IRequest<Common.AppResult<int>>;

public sealed record TogglePromotionStatusCommand(int PropertyId, int PromotionId, bool IsActive) : MediatR.IRequest<Common.AppResult>;
public sealed record DeletePromotionCommand(int PropertyId, int PromotionId) : MediatR.IRequest<Common.AppResult>;
public sealed record GetPromotionsQuery(int PropertyId, bool IncludeDeleted = false) : MediatR.IRequest<Common.AppResult<List<PromotionDto>>>;
