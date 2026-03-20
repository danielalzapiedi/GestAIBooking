namespace GestAI.Web.Dtos;

public sealed record PropertyListItemDto(int Id, string Name, string? CommercialName, int Type, bool IsActive, string? City, string? Country, int UnitsCount);
public sealed class PropertyDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CommercialName { get; set; }
    public int Type { get; set; }
    public bool IsActive { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Country { get; set; }
    public string? Address { get; set; }
    public TimeOnly? DefaultCheckInTime { get; set; }
    public TimeOnly? DefaultCheckOutTime { get; set; }
    public string Currency { get; set; } = "ARS";
    public string? DepositPolicy { get; set; }
    public decimal DefaultDepositPercentage { get; set; }
    public string? CancellationPolicy { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? CheckInInstructions { get; set; }
    public string? PropertyRules { get; set; }
    public string? CommercialContactName { get; set; }
    public string? CommercialContactPhone { get; set; }
    public string? CommercialContactEmail { get; set; }
    public string? PublicSlug { get; set; }
    public string? PublicDescription { get; set; }
}
public sealed record UpsertPropertyCommand(int? PropertyId, string Name, string? CommercialName, int Type, bool IsActive, string? Phone, string? Email, string? City, string? Province, string? Country, string? Address, TimeOnly? DefaultCheckInTime, TimeOnly? DefaultCheckOutTime, string Currency, string? DepositPolicy, decimal DefaultDepositPercentage, string? CancellationPolicy, string? TermsAndConditions, string? CheckInInstructions, string? PropertyRules, string? CommercialContactName, string? CommercialContactPhone, string? CommercialContactEmail, string? PublicSlug, string? PublicDescription);
public sealed record SetupStatusDto(bool HasAnyAccount, int? DefaultAccountId, bool HasAnyProperty, int? DefaultPropertyId, bool HasAnyUnit, int? DefaultUnitId, int? RecommendedPropertyId);
