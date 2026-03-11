using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Guests;

public sealed record SearchGuestsQuery(int PropertyId, string Query) : IRequest<AppResult<List<GuestSearchItemDto>>>;

public sealed class SearchGuestsQueryHandler : IRequestHandler<SearchGuestsQuery, AppResult<List<GuestSearchItemDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public SearchGuestsQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<GuestSearchItemDto>>> Handle(SearchGuestsQuery request, CancellationToken ct)
    {
        var s = (request.Query ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(s))
            return AppResult<List<GuestSearchItemDto>>.Ok(new());

        var data = await _db.Guests.AsNoTracking()
            .Where(g => g.PropertyId == request.PropertyId && (g.Property.Account.OwnerUserId == _current.UserId || g.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && g.IsActive)
            .Where(g => (g.FullName ?? "").ToLower().Contains(s) || (g.Phone ?? "").ToLower().Contains(s) || (g.Email ?? "").ToLower().Contains(s))
            .OrderBy(g => g.FullName)
            .Take(20)
            .Select(g => new GuestSearchItemDto(g.Id, g.FullName, g.Phone, g.Email))
            .ToListAsync(ct);

        return AppResult<List<GuestSearchItemDto>>.Ok(data);
    }
}
