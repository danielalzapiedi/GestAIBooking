using GestAI.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Common;

public sealed record AccessiblePropertyContext(int PropertyId, int AccountId);

public static class PropertyAuthorization
{
    public static Task<AccessiblePropertyContext?> GetAccessiblePropertyAsync(IAppDbContext db, ICurrentUser current, int propertyId, CancellationToken ct)
        => db.Properties.AsNoTracking()
            .Where(x => x.Id == propertyId && (x.Account.OwnerUserId == current.UserId || x.Account.Users.Any(au => au.UserId == current.UserId && au.IsActive)))
            .Select(x => new AccessiblePropertyContext(x.Id, x.AccountId))
            .FirstOrDefaultAsync(ct);
}
