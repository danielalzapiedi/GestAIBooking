using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Units;

public sealed record GetUnitQuery(int PropertyId, int UnitId) : IRequest<AppResult<UnitListItemDto>>;

public sealed class GetUnitQueryHandler : IRequestHandler<GetUnitQuery, AppResult<UnitListItemDto>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public GetUnitQueryHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<UnitListItemDto>> Handle(GetUnitQuery request, CancellationToken ct)
    {
        var unit = await _db.Units.AsNoTracking()
            .Where(u => u.PropertyId == request.PropertyId && u.Id == request.UnitId && (u.Property.Account.OwnerUserId == _current.UserId || u.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .Select(u => new UnitListItemDto(u.Id, u.PropertyId, u.Name, u.CapacityAdults, u.CapacityChildren, u.IsActive, u.BaseRate, u.TotalCapacity, u.ShortDescription, u.DisplayOrder, u.OperationalStatus))
            .FirstOrDefaultAsync(ct);
        return unit is null ? AppResult<UnitListItemDto>.Fail("not_found", "Unidad no encontrada.") : AppResult<UnitListItemDto>.Ok(unit);
    }
}
