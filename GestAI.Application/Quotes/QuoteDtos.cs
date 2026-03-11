using GestAI.Application.Common.Pricing;
using GestAI.Domain.Enums;

namespace GestAI.Application.Quotes;

public sealed record QuoteAvailableUnitDto(
    int UnitId,
    string UnitName,
    decimal BaseAmount,
    decimal PromotionsAmount,
    decimal SuggestedNightlyRate,
    decimal Total,
    decimal SuggestedDepositAmount,
    UnitOperationalStatus OperationalStatus,
    List<string> Rules,
    List<PricingLineDto> Lines,
    List<NightBreakdownDto> NightBreakdown,
    List<AppliedPromotionDto> AppliedPromotions);

public sealed record QuoteResultDto(
    int PropertyId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Nights,
    int Adults,
    int Children,
    decimal SuggestedNightlyRate,
    decimal BaseAmount,
    decimal PromotionsAmount,
    decimal Total,
    decimal SuggestedDepositAmount,
    List<QuoteAvailableUnitDto> AvailableUnits,
    string Summary,
    List<string> ValidationMessages);

public sealed record SaveQuoteCommand(
    int PropertyId,
    int? UnitId,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    string? GuestName,
    string? GuestEmail,
    string? GuestPhone) : MediatR.IRequest<Common.AppResult<SavedQuoteDto>>;

public sealed record SavedQuoteDto(
    int Id,
    string PublicToken,
    int PropertyId,
    int? UnitId,
    string? UnitName,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    int Adults,
    int Children,
    decimal BaseAmount,
    decimal PromotionsAmount,
    decimal TotalAmount,
    decimal SuggestedDepositAmount,
    string? AppliedPromotionNames,
    string? GuestName,
    string? GuestEmail,
    string? GuestPhone,
    SavedQuoteStatus Status,
    DateTime CreatedAtUtc,
    int? CreatedBookingId,
    string PublicUrl,
    string? Summary);

public sealed record GetSavedQuotesQuery(int PropertyId, string? Search, SavedQuoteStatus? Status) : MediatR.IRequest<Common.AppResult<List<SavedQuoteDto>>>;
public sealed record GetSavedQuoteDetailQuery(int PropertyId, int SavedQuoteId) : MediatR.IRequest<Common.AppResult<SavedQuoteDto>>;
public sealed record ConvertSavedQuoteToBookingCommand(int PropertyId, int SavedQuoteId, int GuestId, BookingStatus Status, string? Notes) : MediatR.IRequest<Common.AppResult<int>>;
public sealed record PricingSimulationDto(int PropertyId, int UnitId, string UnitName, QuoteResultDto Quote, List<string> ActiveRestrictions);
public sealed record PricingSimulationQuery(int PropertyId, int UnitId, DateOnly CheckInDate, DateOnly CheckOutDate, int Adults, int Children) : MediatR.IRequest<Common.AppResult<PricingSimulationDto>>;
