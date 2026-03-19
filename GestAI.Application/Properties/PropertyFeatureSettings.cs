using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

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
    bool UseSimpleGuestMode);

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
        var property = await _db.Properties.AsNoTracking()
            .Where(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(x => new { x.Id, x.AccountId })
            .FirstOrDefaultAsync(ct);

        if (property is null)
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        var settings = await _featureService.GetSettingsAsync(request.PropertyId, ct);
        return AppResult<PropertyFeatureSettingsDto>.Ok(Map(settings));
    }

    public async Task<AppResult<PropertyFeatureSettingsDto>> Handle(UpdatePropertyFeatureSettingsCommand request, CancellationToken ct)
    {
        var property = await _db.Properties.AsNoTracking()
            .Where(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(x => new { x.Id, x.AccountId })
            .FirstOrDefaultAsync(ct);

        if (property is null)
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Configuration, ct))
            return AppResult<PropertyFeatureSettingsDto>.Fail("forbidden", "No tenés permisos para administrar la configuración funcional.");

        if (!request.EnableQuotes && request.EnableSavedQuotes)
            return AppResult<PropertyFeatureSettingsDto>.Fail("invalid_state", "No podés habilitar cotizaciones guardadas si el cotizador está deshabilitado.");

        foreach (var validation in GetModuleGovernedFeatureValidations(request))
        {
            if (!validation.Enabled)
                continue;

            if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, validation.RequiredModule, ct))
                return AppResult<PropertyFeatureSettingsDto>.Fail("module_disabled", $"No podés habilitar {validation.Label} porque el plan o rol actual no tiene acceso al módulo {validation.ModuleLabel}.");
        }

        var settings = await _featureService.GetSettingsAsync(request.PropertyId, ct);
        var before = Map(settings);

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

        foreach (var change in DescribeChanges(before, Map(settings)))
            await _audit.WriteAsync(property.AccountId, request.PropertyId, nameof(Domain.Entities.PropertyFeatureSettings), settings.Id, "FeatureChanged", change, ct);

        return AppResult<PropertyFeatureSettingsDto>.Ok(Map(settings));
    }

    private static PropertyFeatureSettingsDto Map(Domain.Entities.PropertyFeatureSettings s)
        => new(s.EnableHousekeeping, s.EnableAgenda, s.EnableQuotes, s.EnableSavedQuotes, s.EnablePromotions, s.EnableAdvancedRates, s.EnablePayments, s.EnableDirectBooking, s.EnableExternalCalendarSync, s.EnableReports, s.EnableTemplates, s.EnableAuditView, s.UseSimpleGuestMode);

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

    private static IEnumerable<(bool Enabled, SaasModule RequiredModule, string Label, string ModuleLabel)> GetModuleGovernedFeatureValidations(UpdatePropertyFeatureSettingsCommand request)
    {
        yield return (request.EnableHousekeeping, SaasModule.Housekeeping, "housekeeping", "Housekeeping");
        yield return (request.EnableAdvancedRates, SaasModule.Rates, "tarifas avanzadas", "Tarifas");
        yield return (request.EnablePromotions, SaasModule.Promotions, "promociones", "Promociones");
        yield return (request.EnablePayments, SaasModule.Payments, "pagos", "Pagos");
        yield return (request.EnableReports, SaasModule.Reports, "reportes", "Reportes");
    }
}
