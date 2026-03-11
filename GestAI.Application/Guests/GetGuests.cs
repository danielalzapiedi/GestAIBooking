using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Guests;

public sealed record GetGuestsQuery(int PropertyId, string? Search) : IRequest<AppResult<List<GuestDto>>>;

public sealed class GetGuestsQueryHandler : IRequestHandler<GetGuestsQuery, AppResult<List<GuestDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public GetGuestsQueryHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<List<GuestDto>>> Handle(GetGuestsQuery request, CancellationToken ct)
    {
        var q = _db.Guests.AsNoTracking()
            .Where(g => g.PropertyId == request.PropertyId && (g.Property.Account.OwnerUserId == _current.UserId || g.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && g.IsActive);

        var s = (request.Search ?? "").Trim();
        if (!string.IsNullOrWhiteSpace(s))
        {
            s = s.ToLower();
            q = q.Where(g =>
                (g.FullName ?? "").ToLower().Contains(s) ||
                (g.Phone ?? "").ToLower().Contains(s) ||
                (g.Email ?? "").ToLower().Contains(s));
        }

        var data = await q.OrderBy(g => g.FullName)
            .Select(g => new GuestDto(g.Id, g.PropertyId, g.FullName, g.Phone, g.Email, g.DocumentType, g.DocumentNumber, g.Notes, g.IsActive))
            .ToListAsync(ct);

        return AppResult<List<GuestDto>>.Ok(data);
    }
}
