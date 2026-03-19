using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Rates;

public sealed record GetRatesQuery(int PropertyId) : IRequest<AppResult<List<RatePlanDto>>>;

public sealed class GetRatesQueryHandler : IRequestHandler<GetRatesQuery, AppResult<List<RatePlanDto>>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current; private readonly IUserAccessService _access; private readonly IPropertyFeatureService _features;
    public GetRatesQueryHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, IPropertyFeatureService features) { _db = db; _current = current; _access = access; _features = features; }
    public async Task<AppResult<List<RatePlanDto>>> Handle(GetRatesQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.AdvancedRates, ct))
            return AppResult<List<RatePlanDto>>.Fail("feature_disabled", "Las tarifas avanzadas están desactivadas para este hospedaje.");

        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);
        if (property is null)
            return AppResult<List<RatePlanDto>>.Fail("forbidden", "Hospedaje inválido o sin acceso.");
        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Rates, ct))
            return AppResult<List<RatePlanDto>>.Fail("forbidden", "No tenés permisos para usar el módulo de tarifas.");

        var plans = await _db.RatePlans.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Include(x => x.SeasonalRates).Include(x => x.DateRangeRates).Include(x => x.Unit)
            .OrderBy(x => x.Unit.DisplayOrder).ThenBy(x => x.Name)
            .ToListAsync(ct);
        var data = plans.Select(x => new RatePlanDto(x.Id, x.PropertyId, x.UnitId, x.Unit.Name, x.Name, x.BaseNightlyRate, x.WeekendAdjustmentEnabled, x.WeekendAdjustmentType, x.WeekendAdjustmentValue, x.IsActive,
            x.SeasonalRates.OrderBy(s=>s.Name).Select(s => new SeasonalRateDto(s.Id, s.Name, s.StartMonth, s.StartDay, s.EndMonth, s.EndDay, s.AdjustmentType, s.AdjustmentValue, s.IsActive)).ToList(),
            x.DateRangeRates.OrderBy(d=>d.DateFrom).Select(d => new DateRangeRateDto(d.Id, d.Name, d.DateFrom, d.DateTo, d.AdjustmentType, d.AdjustmentValue, d.IsActive)).ToList())).ToList();
        return AppResult<List<RatePlanDto>>.Ok(data);
    }
}
