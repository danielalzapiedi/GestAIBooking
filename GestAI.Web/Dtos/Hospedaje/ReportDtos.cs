namespace GestAI.Web.Dtos;

public sealed record IncomeReportDto(DateOnly From, DateOnly ToExclusive, decimal PaidIncome, decimal DepositsCollected, decimal DepositsPending, decimal AverageIncomePerNight);

public sealed record OccupancyReportDto(DateOnly From, DateOnly ToExclusive, int TotalNightsAvailable, int NightsOccupied, decimal OccupancyPercent, decimal AverageStayNights);

public sealed record ReportCountItemDto(string Label, int Count);

public sealed record UnitOccupancyItemDto(int UnitId, string UnitName, int NightsOccupied, int NightsAvailable, decimal OccupancyPercent, decimal Income, decimal AverageIncomePerNight);

public sealed record ReportsDto(IncomeReportDto Income, OccupancyReportDto Occupancy, int TotalBookings, List<ReportCountItemDto> BookingsByStatus, List<UnitOccupancyItemDto> UnitOccupancy);
