using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

public sealed record GetMyPropertiesQuery : IRequest<AppResult<List<PropertyListItemDto>>>;

public sealed class GetMyPropertiesQueryHandler : IRequestHandler<GetMyPropertiesQuery, AppResult<List<PropertyListItemDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetMyPropertiesQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<PropertyListItemDto>>> Handle(GetMyPropertiesQuery request, CancellationToken ct)
    {
        var props = await _db.Properties.AsNoTracking()
            .Where(p => (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .OrderBy(p => p.Name)
            .Select(p => new PropertyListItemDto(
                p.Id,
                p.Name,
                p.CommercialName,
                p.Type,
                p.IsActive,
                p.City,
                p.Country,
                p.Units.Count(u => u.IsActive)))
            .ToListAsync(ct);

        return AppResult<List<PropertyListItemDto>>.Ok(props);
    }
}
