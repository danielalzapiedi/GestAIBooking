using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class PropertyFeatureSettings : Entity
{
    public int PropertyId { get; set; }
    public Property Property { get; set; } = null!;

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
    public bool UseSimpleGuestMode { get; set; } = false;
}
