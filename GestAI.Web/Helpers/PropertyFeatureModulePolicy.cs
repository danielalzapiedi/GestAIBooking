using GestAI.Web.Dtos;

namespace GestAI.Web.Helpers;

public static class PropertyFeatureModulePolicy
{
    public static SaasModule? GetRequiredModule(string featureKey)
        => featureKey switch
        {
            nameof(PropertyFeatureSettingsDto.EnableHousekeeping) => SaasModule.Housekeeping,
            nameof(PropertyFeatureSettingsDto.EnableAdvancedRates) => SaasModule.Rates,
            nameof(PropertyFeatureSettingsDto.EnablePromotions) => SaasModule.Promotions,
            nameof(PropertyFeatureSettingsDto.EnablePayments) => SaasModule.Payments,
            nameof(PropertyFeatureSettingsDto.EnableReports) => SaasModule.Reports,
            _ => null
        };

    public static bool IsAvailable(PropertyFeatureSettingsDto? settings, string featureKey)
    {
        var requiredModule = GetRequiredModule(featureKey);
        if (!requiredModule.HasValue)
            return true;

        var availability = settings?.ModuleAvailability?.FirstOrDefault(x => x.FeatureKey == featureKey);
        return availability?.AvailableByPlan ?? true;
    }
}
