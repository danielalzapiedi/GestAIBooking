using GestAI.Domain.Enums;

namespace GestAI.Application.Properties;

public sealed record PropertyFeatureModuleDefinition(
    string FeatureKey,
    PropertyFeature Feature,
    SaasModule RequiredModule,
    string FeatureLabel,
    string ModuleLabel);

public static class PropertyFeatureModulePolicy
{
    private static readonly IReadOnlyList<PropertyFeatureModuleDefinition> GovernedFeatures =
    [
        new(nameof(PropertyFeatureSettingsDto.EnableHousekeeping), PropertyFeature.Housekeeping, SaasModule.Housekeeping, "housekeeping", "Housekeeping"),
        new(nameof(PropertyFeatureSettingsDto.EnableAdvancedRates), PropertyFeature.AdvancedRates, SaasModule.Rates, "tarifas avanzadas", "Tarifas"),
        new(nameof(PropertyFeatureSettingsDto.EnablePromotions), PropertyFeature.Promotions, SaasModule.Promotions, "promociones", "Promociones"),
        new(nameof(PropertyFeatureSettingsDto.EnablePayments), PropertyFeature.Payments, SaasModule.Payments, "pagos", "Pagos"),
        new(nameof(PropertyFeatureSettingsDto.EnableReports), PropertyFeature.Reports, SaasModule.Reports, "reportes", "Reportes")
    ];

    public static IReadOnlyList<PropertyFeatureModuleDefinition> GetGovernedFeatures() => GovernedFeatures;

    public static SaasModule? GetRequiredModule(PropertyFeature feature)
        => GovernedFeatures.FirstOrDefault(x => x.Feature == feature)?.RequiredModule;
}
