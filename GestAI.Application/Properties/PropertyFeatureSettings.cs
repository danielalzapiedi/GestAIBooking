using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Application.Saas;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

public sealed record PropertyFeatureModuleAvailabilityDto(
    string FeatureKey,
    SaasModule RequiredModule,
    string FeatureLabel,
    string ModuleLabel,
    bool AvailableByPlan);

public sealed record PropertyFeatureSettingsDto(
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
    bool UseSimpleGuestMode,
    List<PropertyFeatureModuleAvailabilityDto>? ModuleAvailability = null);

public sealed record GetPropertyFeatureSettingsQuery(int PropertyId) : IRequest<AppResult<PropertyFeatureSettingsDto>>;

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
    bool UseSimpleGuestMode) : IRequest<AppResult<PropertyFeatureSettingsDto>>;

public sealed class PropertyFeatureSettingsHandler :
    IRequestHandler<GetPropertyFeatureSettingsQuery, AppResult<PropertyFeatureSettingsDto>>,
    IRequestHandler<UpdatePropertyFeatureSettingsCommand, AppResult<PropertyFeatureSettingsDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _featureService;
    private readonly IAuditService _audit;

    public PropertyFeatureSettingsHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, IPropertyFeatureService featureService, IAuditService audit)
    {
        _db = db;
        _current = current;
        _access = access;
        _featureService = featureService;
        _audit = audit;
    }

    public async Task<AppResult<PropertyFeatureSettingsDto>> Handle(GetPropertyFeatureSettingsQuery request, CancellationToken ct)
    {
        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);

        if (property is null)
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        var settings = await _featureService.GetSettingsAsync(request.PropertyId, ct);
        return AppResult<PropertyFeatureSettingsDto>.Ok(await MapAsync(settings, property.AccountId, ct));
    }

    public async Task<AppResult<PropertyFeatureSettingsDto>> Handle(UpdatePropertyFeatureSettingsCommand request, CancellationToken ct)
    {
        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);

        if (property is null)
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Configuration, ct))
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "No tenés permisos para administrar la configuración funcional.");

        if (!request.EnableQuotes && request.EnableSavedQuotes)
            return AppResult<PropertyFeatureSettingsDto>.Fail("invalid_state", "No podés habilitar cotizaciones guardadas si el cotizador está deshabilitado.");

        var availability = await GetModuleAvailabilityAsync(property.AccountId, ct);
        foreach (var validation in availability.Where(x => IsFeatureEnabled(request, x.FeatureKey)))
        {
            if (!validation.AvailableByPlan)
                return AppResult<PropertyFeatureSettingsDto>.Fail("module_disabled", $"No podés habilitar {validation.FeatureLabel} porque el plan actual no incluye el módulo {validation.ModuleLabel}.");
        }

        var settings = await _featureService.GetSettingsAsync(request.PropertyId, ct);
        var before = await MapAsync(settings, property.AccountId, ct);

        settings.EnableHousekeeping = request.EnableHousekeeping;
        settings.EnableAgenda = request.EnableAgenda;
        settings.EnableQuotes = request.EnableQuotes;
        settings.EnableSavedQuotes = request.EnableSavedQuotes;
        settings.EnablePromotions = request.EnablePromotions;
        settings.EnableAdvancedRates = request.EnableAdvancedRates;
        settings.EnablePayments = request.EnablePayments;
        settings.EnableDirectBooking = request.EnableDirectBooking;
        settings.EnableExternalCalendarSync = request.EnableExternalCalendarSync;
        settings.EnableReports = request.EnableReports;
        settings.EnableTemplates = request.EnableTemplates;
        settings.EnableAuditView = request.EnableAuditView;
        settings.UseSimpleGuestMode = request.UseSimpleGuestMode;

        await _db.SaveChangesAsync(ct);

        var after = await MapAsync(settings, property.AccountId, ct);
        foreach (var change in DescribeChanges(before, after))
            await _audit.WriteAsync(property.AccountId, request.PropertyId, nameof(Domain.Entities.PropertyFeatureSettings), settings.Id, "FeatureChanged", change, ct);

        return AppResult<PropertyFeatureSettingsDto>.Ok(after);
    }

    private async Task<PropertyFeatureSettingsDto> MapAsync(Domain.Entities.PropertyFeatureSettings settings, int accountId, CancellationToken ct)
        => new(
            settings.EnableHousekeeping,
            settings.EnableAgenda,
            settings.EnableQuotes,
            settings.EnableSavedQuotes,
            settings.EnablePromotions,
            settings.EnableAdvancedRates,
            settings.EnablePayments,
            settings.EnableDirectBooking,
            settings.EnableExternalCalendarSync,
            settings.EnableReports,
            settings.EnableTemplates,
            settings.EnableAuditView,
            settings.UseSimpleGuestMode,
            await GetModuleAvailabilityAsync(accountId, ct));

    private static IEnumerable<string> DescribeChanges(PropertyFeatureSettingsDto before, PropertyFeatureSettingsDto after)
    {
        var pairs = new Dictionary<string, (bool Before, bool After)>
        {
            [nameof(before.EnableHousekeeping)] = (before.EnableHousekeeping, after.EnableHousekeeping),
            [nameof(before.EnableAgenda)] = (before.EnableAgenda, after.EnableAgenda),
            [nameof(before.EnableQuotes)] = (before.EnableQuotes, after.EnableQuotes),
            [nameof(before.EnableSavedQuotes)] = (before.EnableSavedQuotes, after.EnableSavedQuotes),
            [nameof(before.EnablePromotions)] = (before.EnablePromotions, after.EnablePromotions),
            [nameof(before.EnableAdvancedRates)] = (before.EnableAdvancedRates, after.EnableAdvancedRates),
            [nameof(before.EnablePayments)] = (before.EnablePayments, after.EnablePayments),
            [nameof(before.EnableDirectBooking)] = (before.EnableDirectBooking, after.EnableDirectBooking),
            [nameof(before.EnableExternalCalendarSync)] = (before.EnableExternalCalendarSync, after.EnableExternalCalendarSync),
            [nameof(before.EnableReports)] = (before.EnableReports, after.EnableReports),
            [nameof(before.EnableTemplates)] = (before.EnableTemplates, after.EnableTemplates),
            [nameof(before.EnableAuditView)] = (before.EnableAuditView, after.EnableAuditView),
            [nameof(before.UseSimpleGuestMode)] = (before.UseSimpleGuestMode, after.UseSimpleGuestMode)
        };

        return pairs
            .Where(x => x.Value.Before != x.Value.After)
            .Select(x => $"Feature {x.Key} changed from {x.Value.Before.ToString().ToLowerInvariant()} to {x.Value.After.ToString().ToLowerInvariant()}");
    }

    private async Task<List<PropertyFeatureModuleAvailabilityDto>> GetModuleAvailabilityAsync(int accountId, CancellationToken ct)
    {
        var plan = await _db.Accounts.AsNoTracking()
            .Where(x => x.Id == accountId && x.IsActive)
            .Select(x => x.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.StartedAtUtc)
                .Select(p => p.PlanDefinition)
                .FirstOrDefault())
            .FirstOrDefaultAsync(ct);

        return PropertyFeatureModulePolicy.GetGovernedFeatures()
            .Select(definition => new PropertyFeatureModuleAvailabilityDto(
                definition.FeatureKey,
                definition.RequiredModule,
                definition.FeatureLabel,
                definition.ModuleLabel,
                SaasPermissionMap.IsEnabledByPlan(plan, definition.RequiredModule)))
            .ToList();
    }

    private static bool IsFeatureEnabled(UpdatePropertyFeatureSettingsCommand request, string featureKey)
        => featureKey switch
        {
            nameof(PropertyFeatureSettingsDto.EnableHousekeeping) => request.EnableHousekeeping,
            nameof(PropertyFeatureSettingsDto.EnableAdvancedRates) => request.EnableAdvancedRates,
            nameof(PropertyFeatureSettingsDto.EnablePromotions) => request.EnablePromotions,
            nameof(PropertyFeatureSettingsDto.EnablePayments) => request.EnablePayments,
            nameof(PropertyFeatureSettingsDto.EnableReports) => request.EnableReports,
            _ => false
        };
}
