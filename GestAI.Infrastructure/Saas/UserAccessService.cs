using GestAI.Application.Abstractions;
using GestAI.Application.Saas;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Infrastructure.Saas;

public sealed class UserAccessService : IUserAccessService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public UserAccessService(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<int?> GetCurrentAccountIdAsync(CancellationToken ct)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _current.UserId, ct);
        if (user?.DefaultAccountId > 0) return user.DefaultAccountId;

        var membershipAccountId = await _db.AccountUsers.AsNoTracking()
            .Where(x => x.UserId == _current.UserId && x.IsActive)
            .OrderBy(x => x.AccountId)
            .Select(x => (int?)x.AccountId)
            .FirstOrDefaultAsync(ct);

        if (membershipAccountId.HasValue) return membershipAccountId;

        return await _db.Accounts.AsNoTracking()
            .Where(x => x.OwnerUserId == _current.UserId && x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int?> GetDefaultPropertyIdAsync(CancellationToken ct)
        => await _db.Users.AsNoTracking().Where(x => x.Id == _current.UserId).Select(x => x.DefaultPropertyId).FirstOrDefaultAsync(ct);

    public async Task<bool> HasPropertyAccessAsync(int propertyId, CancellationToken ct)
        => await _db.Properties.AsNoTracking().AnyAsync(x => x.Id == propertyId &&
            (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(u => u.UserId == _current.UserId && u.IsActive)), ct);

    public async Task<bool> HasPropertyModuleAccessAsync(int propertyId, SaasModule module, CancellationToken ct)
    {
        var accountId = await _db.Properties.AsNoTracking()
            .Where(x => x.Id == propertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(u => u.UserId == _current.UserId && u.IsActive)))
            .Select(x => (int?)x.AccountId)
            .FirstOrDefaultAsync(ct);

        if (!accountId.HasValue)
            return false;

        return await HasModuleAccessAsync(accountId.Value, module, ct);
    }

    public async Task<AccountUser?> GetMembershipAsync(int accountId, CancellationToken ct)
        => await _db.AccountUsers.AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == accountId && x.UserId == _current.UserId && x.IsActive, ct);

    public async Task<bool> HasModuleAccessAsync(int accountId, SaasModule module, CancellationToken ct)
    {
        var account = await _db.Accounts.AsNoTracking()
            .Where(x => x.Id == accountId && x.IsActive)
            .Select(x => new
            {
                IsOwner = x.OwnerUserId == _current.UserId,
                Membership = x.Users.Where(u => u.UserId == _current.UserId && u.IsActive).Select(u => new { u.Role }).FirstOrDefault(),
                Plan = x.SubscriptionPlans.Where(p => p.IsActive).OrderByDescending(p => p.StartedAtUtc)
                    .Select(p => new SaasPlanDefinition
                    {
                        Id = p.PlanDefinition.Id,
                        Code = p.PlanDefinition.Code,
                        Name = p.PlanDefinition.Name,
                        MaxProperties = p.PlanDefinition.MaxProperties,
                        MaxUnits = p.PlanDefinition.MaxUnits,
                        MaxUsers = p.PlanDefinition.MaxUsers,
                        IncludesOperations = p.PlanDefinition.IncludesOperations,
                        IncludesPublicPortal = p.PlanDefinition.IncludesPublicPortal,
                        IncludesReports = p.PlanDefinition.IncludesReports
                    }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (account is null) return false;
        if (!account.IsOwner && account.Membership is null) return false;

        var role = account.IsOwner ? InternalUserRole.Owner : account.Membership!.Role;
        return SaasPermissionMap.HasAccess(role, account.Plan, module, account.IsOwner);
    }
}
