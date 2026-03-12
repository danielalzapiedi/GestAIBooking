using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Infrastructure.Saas;

public sealed class PropertyFeatureService : IPropertyFeatureService
{
    private readonly IAppDbContext _db;

    public PropertyFeatureService(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<PropertyFeatureSettings> GetSettingsAsync(int propertyId, CancellationToken ct)
    {
        var settings = await _db.PropertyFeatureSettings.FirstOrDefaultAsync(x => x.PropertyId == propertyId, ct);
        if (settings is not null)
            return settings;

        settings = new PropertyFeatureSettings { PropertyId = propertyId };
        _db.PropertyFeatureSettings.Add(settings);
        await _db.SaveChangesAsync(ct);
        return settings;
    }

    public async Task<bool> IsEnabledAsync(int propertyId, PropertyFeature feature, CancellationToken ct)
    {
        var settings = await GetSettingsAsync(propertyId, ct);
        return feature switch
        {
            PropertyFeature.Housekeeping => settings.EnableHousekeeping,
            PropertyFeature.Agenda => settings.EnableAgenda,
            PropertyFeature.Quotes => settings.EnableQuotes,
            PropertyFeature.SavedQuotes => settings.EnableSavedQuotes,
            PropertyFeature.Promotions => settings.EnablePromotions,
            PropertyFeature.AdvancedRates => settings.EnableAdvancedRates,
            PropertyFeature.Payments => settings.EnablePayments,
            PropertyFeature.DirectBooking => settings.EnableDirectBooking,
            PropertyFeature.ExternalCalendarSync => settings.EnableExternalCalendarSync,
            PropertyFeature.Reports => settings.EnableReports,
            PropertyFeature.Templates => settings.EnableTemplates,
            PropertyFeature.AuditView => settings.EnableAuditView,
            PropertyFeature.SimpleGuestMode => settings.UseSimpleGuestMode,
            _ => true
        };
    }
}
