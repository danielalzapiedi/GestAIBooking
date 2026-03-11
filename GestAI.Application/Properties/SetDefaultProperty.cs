using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Properties;

public sealed record SetDefaultPropertyCommand(int PropertyId) : IRequest<AppResult>;

public sealed class SetDefaultPropertyCommandValidator : AbstractValidator<SetDefaultPropertyCommand>
{
    public SetDefaultPropertyCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
    }
}

public sealed class SetDefaultPropertyCommandHandler : IRequestHandler<SetDefaultPropertyCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public SetDefaultPropertyCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(SetDefaultPropertyCommand request, CancellationToken ct)
    {
        var prop = await _db.Properties
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && p.IsActive, ct);

        if (prop is null)
            return AppResult.Fail("not_found", "Hospedaje inexistente o sin acceso.");

        var user = await _db.Users.FirstAsync(x => x.Id == _current.UserId, ct);
        user.DefaultPropertyId = request.PropertyId;

        // Si no tiene default account, lo seteamos al Account de la property
        if (user.DefaultAccountId == 0)
            user.DefaultAccountId = prop.AccountId;

        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
