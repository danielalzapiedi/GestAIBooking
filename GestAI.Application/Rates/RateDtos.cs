using GestAI.Domain.Enums;

namespace GestAI.Application.Rates;

public sealed record SeasonalRateDto(int Id, string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record DateRangeRateDto(int Id, string Name, DateOnly DateFrom, DateOnly DateTo, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record RatePlanDto(int Id, int PropertyId, int UnitId, string UnitName, string Name, decimal BaseNightlyRate, bool WeekendAdjustmentEnabled, RateAdjustmentType WeekendAdjustmentType, decimal WeekendAdjustmentValue, bool IsActive, List<SeasonalRateDto> SeasonalRates, List<DateRangeRateDto> DateRangeRates);
