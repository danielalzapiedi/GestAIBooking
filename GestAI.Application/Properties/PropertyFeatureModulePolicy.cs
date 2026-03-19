using GestAI.Domain.Enums;

namespace GestAI.Application.Properties;

public static class PropertyFeatureModulePolicy
{
    public static SaasModule? GetRequiredModule(PropertyFeature feature)
        => feature switch
        {
            PropertyFeature.Housekeeping => SaasModule.Housekeeping,
            PropertyFeature.AdvancedRates => SaasModule.Rates,
            PropertyFeature.Promotions => SaasModule.Promotions,
            PropertyFeature.Payments => SaasModule.Payments,
            PropertyFeature.Reports => SaasModule.Reports,
            _ => null
        };
}
