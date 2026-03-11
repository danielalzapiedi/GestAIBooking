namespace GestAI.Web.Dtos;

public sealed record SeasonalRateDto(int Id, string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record DateRangeRateDto(int Id, string Name, DateOnly DateFrom, DateOnly DateTo, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record RatePlanDto(int Id, int PropertyId, int UnitId, string UnitName, string Name, decimal BaseNightlyRate, bool WeekendAdjustmentEnabled, RateAdjustmentType WeekendAdjustmentType, decimal WeekendAdjustmentValue, bool IsActive, List<SeasonalRateDto> SeasonalRates, List<DateRangeRateDto> DateRangeRates);
public sealed record SeasonalRateInputDto(string Name, int StartMonth, int StartDay, int EndMonth, int EndDay, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record DateRangeRateInputDto(string Name, DateOnly DateFrom, DateOnly DateTo, RateAdjustmentType AdjustmentType, decimal AdjustmentValue, bool IsActive);
public sealed record UpsertRatePlanCommand(int PropertyId, int? RatePlanId, int UnitId, string Name, decimal BaseNightlyRate, bool WeekendAdjustmentEnabled, RateAdjustmentType WeekendAdjustmentType, decimal WeekendAdjustmentValue, bool IsActive, List<SeasonalRateInputDto>? SeasonalRates, List<DateRangeRateInputDto>? DateRangeRates);
