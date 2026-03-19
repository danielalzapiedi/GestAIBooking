namespace GestAI.Web.Dtos;

public sealed record PropertyFeatureModuleAvailabilityDto(string FeatureKey, SaasModule RequiredModule, string FeatureLabel, string ModuleLabel, bool AvailableByPlan);

public sealed class PropertyFeatureSettingsDto
{
    public bool EnableHousekeeping { get; set; } = true;
    public bool EnableAgenda { get; set; } = true;
    public bool EnableQuotes { get; set; } = true;
    public bool EnableSavedQuotes { get; set; } = true;
    public bool EnablePromotions { get; set; } = true;
    public bool EnableAdvancedRates { get; set; } = true;
    public bool EnablePayments { get; set; } = true;
    public bool EnableDirectBooking { get; set; } = true;
    public bool EnableExternalCalendarSync { get; set; } = true;
    public bool EnableReports { get; set; } = true;
    public bool EnableTemplates { get; set; } = true;
    public bool EnableAuditView { get; set; } = true;
    public bool UseSimpleGuestMode { get; set; }
    public List<PropertyFeatureModuleAvailabilityDto> ModuleAvailability { get; set; } = [];
}

public sealed record UpdatePropertyFeatureSettingsCommand(
    int PropertyId,
    bool EnableHousekeeping,
    bool EnableAgenda,
    bool EnableQuotes,
    bool EnableSavedQuotes,
    bool EnablePromotions,
    bool EnableAdvancedRates,
    bool EnablePayments,
    bool EnableDirectBooking,
    bool EnableExternalCalendarSync,
    bool EnableReports,
    bool EnableTemplates,
    bool EnableAuditView,
    bool UseSimpleGuestMode);
