namespace GestAI.Application.Properties;

public sealed record PropertyListItemDto(
    int Id,
    string Name,
    string? CommercialName,
    int Type,
    bool IsActive,
    string? City,
    string? Country,
    int UnitsCount);

public sealed record PropertyDetailDto(
    int Id,
    string Name,
    string? CommercialName,
    int Type,
    bool IsActive,
    string? Phone,
    string? Email,
    string? City,
    string? Province,
    string? Country,
    string? Address,
    TimeOnly? DefaultCheckInTime,
    TimeOnly? DefaultCheckOutTime,
    string Currency,
    string? DepositPolicy,
    decimal DefaultDepositPercentage,
    string? CancellationPolicy,
    string? TermsAndConditions,
    string? CheckInInstructions,
    string? PropertyRules,
    string? CommercialContactName,
    string? CommercialContactPhone,
    string? CommercialContactEmail,
    string? PublicSlug,
    string? PublicDescription);
