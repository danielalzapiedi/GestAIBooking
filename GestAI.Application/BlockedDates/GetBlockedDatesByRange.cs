using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.BlockedDates;

public sealed record GetBlockedDatesByRangeQuery(int PropertyId, DateOnly From, DateOnly ToExclusive) : IRequest<AppResult<List<BlockedDateDto>>>;

public sealed class GetBlockedDatesByRangeQueryHandler : IRequestHandler<GetBlockedDatesByRangeQuery, AppResult<List<BlockedDateDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetBlockedDatesByRangeQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<BlockedDateDto>>> Handle(GetBlockedDatesByRangeQuery request, CancellationToken ct)
    {
        var data = await _db.BlockedDates.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Where(b => b.DateFrom < request.ToExclusive && request.From < b.DateTo)
            .OrderBy(x => x.UnitId).ThenBy(x => x.DateFrom)
            .Select(x => new BlockedDateDto(x.Id, x.PropertyId, x.UnitId, x.DateFrom, x.DateTo, x.Reason))
            .ToListAsync(ct);

        return AppResult<List<BlockedDateDto>>.Ok(data);
    }
}
