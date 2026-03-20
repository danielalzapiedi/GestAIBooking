using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Setup;

public sealed record SetupStatusDto(
    bool HasAnyAccount, int? DefaultAccountId,
    bool HasAnyProperty, int? DefaultPropertyId,
    bool HasAnyUnit, int? DefaultUnitId,
    int? RecommendedPropertyId);

public sealed record GetSetupStatusQuery : IRequest<AppResult<SetupStatusDto>>;

public sealed class GetSetupStatusQueryHandler : IRequestHandler<GetSetupStatusQuery, AppResult<SetupStatusDto>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetSetupStatusQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<SetupStatusDto>> Handle(GetSetupStatusQuery request, CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _current.UserId, ct);
        var defaultPropertyId = user?.DefaultPropertyId;
        var defaultAccountId = user?.DefaultAccountId;

        var activePropertyIds = await _db.Properties.AsNoTracking()
            .Where(p => (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && p.IsActive)
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .ToListAsync(ct);

        var hasAnyAccount = await _db.Accounts.AsNoTracking().AnyAsync(a => (a.OwnerUserId == _current.UserId || a.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && a.IsActive, ct);
        var hasAnyProperty = activePropertyIds.Count > 0;
        var hasAnyUnit = await _db.Units.AsNoTracking().AnyAsync(u => (u.Property.Account.OwnerUserId == _current.UserId || u.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && u.IsActive, ct);
        var recommendedPropertyId = defaultPropertyId ?? (activePropertyIds.Count == 1 ? activePropertyIds[0] : null);

        int? defaultUnitId = null;
        if (recommendedPropertyId is not null)
        {
            defaultUnitId = await _db.Units.AsNoTracking()
                .Where(u => u.PropertyId == recommendedPropertyId.Value && u.IsActive)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync(ct);
        }

        return AppResult<SetupStatusDto>.Ok(new SetupStatusDto(
            HasAnyAccount: hasAnyAccount, DefaultAccountId: defaultAccountId == 0 ? null : defaultAccountId,
            HasAnyProperty: hasAnyProperty, DefaultPropertyId: defaultPropertyId,
            HasAnyUnit: hasAnyUnit, DefaultUnitId: defaultUnitId,
            RecommendedPropertyId: recommendedPropertyId
        ));
    }
}
