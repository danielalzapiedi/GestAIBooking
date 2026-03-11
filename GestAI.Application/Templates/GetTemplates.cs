using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Templates;

public sealed record GetTemplatesQuery(int PropertyId) : IRequest<AppResult<List<MessageTemplateDto>>>;

public sealed class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, AppResult<List<MessageTemplateDto>>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public GetTemplatesQueryHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult<List<MessageTemplateDto>>> Handle(GetTemplatesQuery request, CancellationToken ct)
    {
        var data = await _db.MessageTemplates.AsNoTracking().Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)))
            .OrderBy(x => x.Type).ThenBy(x => x.Name)
            .Select(x => new MessageTemplateDto(x.Id, x.PropertyId, x.Type, x.Name, x.Body, x.IsActive)).ToListAsync(ct);
        return AppResult<List<MessageTemplateDto>>.Ok(data);
    }
}
