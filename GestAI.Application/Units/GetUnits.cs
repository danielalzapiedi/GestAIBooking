using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Units;

public sealed record GetUnitsQuery(int PropertyId) : IRequest<AppResult<List<UnitListItemDto>>>;

public sealed class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, AppResult<List<UnitListItemDto>>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public GetUnitsQueryHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<List<UnitListItemDto>>> Handle(GetUnitsQuery request, CancellationToken ct)
    {
        var units = await _db.Units.AsNoTracking()
            .Where(u => u.PropertyId == request.PropertyId && (u.Property.Account.OwnerUserId == _current.UserId || u.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .OrderBy(u => u.DisplayOrder).ThenBy(u => u.Name)
            .Select(u => new UnitListItemDto(u.Id, u.PropertyId, u.Name, u.CapacityAdults, u.CapacityChildren, u.IsActive, u.BaseRate, u.TotalCapacity, u.ShortDescription, u.DisplayOrder, u.OperationalStatus))
            .ToListAsync(ct);
        return AppResult<List<UnitListItemDto>>.Ok(units);
    }
}
