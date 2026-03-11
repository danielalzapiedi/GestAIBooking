namespace GestAI.Application.Dashboard;

public sealed record DashboardMonthPointDto(string Label, decimal Value);
public sealed record DashboardBookingStateDto(string Status, int Count);
public sealed record DashboardUpcomingBookingDto(int BookingId, string BookingCode, string GuestName, string UnitName, DateOnly CheckInDate, DateOnly CheckOutDate, decimal PendingAmount);
public sealed record DashboardSummaryDto(decimal OccupancyPercentMonth, decimal CollectedIncomeMonth, int CheckInsToday, int CheckOutsToday, decimal PendingBalanceTotal, List<DashboardBookingStateDto> BookingsByStatus, List<DashboardMonthPointDto> IncomeByMonth, List<DashboardMonthPointDto> OccupancyByMonth, List<DashboardUpcomingBookingDto> UpcomingBookings);
