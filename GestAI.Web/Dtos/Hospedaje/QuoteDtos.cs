namespace GestAI.Web.Dtos;

public sealed record NightBreakdownDto(DateOnly Date, decimal BaseRate, List<PricingLineDto> Adjustments, decimal FinalRate);
public sealed record AppliedPromotionDto(int PromotionId, string Name, decimal Amount, bool IsCumulative, int Priority);

public sealed record QuoteAvailableUnitDto(int UnitId, string UnitName, decimal BaseAmount, decimal PromotionsAmount, decimal SuggestedNightlyRate, decimal Total, decimal SuggestedDepositAmount, UnitOperationalStatus OperationalStatus, List<string> Rules, List<PricingLineDto> Lines, List<NightBreakdownDto> NightBreakdown, List<AppliedPromotionDto> AppliedPromotions);
public sealed record QuoteResultDto(int PropertyId, DateOnly CheckInDate, DateOnly CheckOutDate, int Nights, int Adults, int Children, decimal SuggestedNightlyRate, decimal BaseAmount, decimal PromotionsAmount, decimal Total, decimal SuggestedDepositAmount, List<QuoteAvailableUnitDto> AvailableUnits, string Summary, List<string> ValidationMessages);
public sealed record SaveQuoteCommand(int PropertyId, int? UnitId, DateOnly CheckInDate, DateOnly CheckOutDate, int Adults, int Children, string? GuestName, string? GuestEmail, string? GuestPhone);
public sealed record SavedQuoteDto(int Id, string PublicToken, int PropertyId, int? UnitId, string? UnitName, DateOnly CheckInDate, DateOnly CheckOutDate, int Adults, int Children, decimal BaseAmount, decimal PromotionsAmount, decimal TotalAmount, decimal SuggestedDepositAmount, string? AppliedPromotionNames, string? GuestName, string? GuestEmail, string? GuestPhone, SavedQuoteStatus Status, DateTime CreatedAtUtc, int? CreatedBookingId, string PublicUrl, string? Summary);
public sealed record ConvertSavedQuoteToBookingCommand(int PropertyId, int SavedQuoteId, int GuestId, BookingStatus Status, string? Notes);
public sealed record PricingSimulationDto(int PropertyId, int UnitId, string UnitName, QuoteResultDto Quote, List<string> ActiveRestrictions);
