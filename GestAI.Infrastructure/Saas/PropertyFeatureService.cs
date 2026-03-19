using GestAI.Application.Abstractions;
using GestAI.Application.Properties;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Infrastructure.Saas;

public sealed class PropertyFeatureService : IPropertyFeatureService
{
    private readonly IAppDbContext _db;
    private readonly IUserAccessService _access;

    public PropertyFeatureService(IAppDbContext db, IUserAccessService access)
    {
        _db = db;
        _access = access;
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
        var enabled = feature switch
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

        if (!enabled)
            return false;

        var requiredModule = PropertyFeatureModulePolicy.GetRequiredModule(feature);
        if (!requiredModule.HasValue)
            return true;

        var accountId = await _db.Properties.AsNoTracking()
            .Where(x => x.Id == propertyId)
            .Select(x => (int?)x.AccountId)
            .FirstOrDefaultAsync(ct);

        if (!accountId.HasValue)
            return false;

        return await _access.HasModuleAccessAsync(accountId.Value, requiredModule.Value, ct);
    }
}
