using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.BlockedDates;

public sealed record DeleteBlockedDateCommand(int PropertyId, int BlockedDateId) : IRequest<AppResult>;

public sealed class DeleteBlockedDateCommandValidator : AbstractValidator<DeleteBlockedDateCommand>
{
    public DeleteBlockedDateCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.BlockedDateId).GreaterThan(0);
    }
}

public sealed class DeleteBlockedDateCommandHandler : IRequestHandler<DeleteBlockedDateCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public DeleteBlockedDateCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(DeleteBlockedDateCommand request, CancellationToken ct)
    {
        var entity = await _db.BlockedDates
            .FirstOrDefaultAsync(x => x.PropertyId == request.PropertyId && x.Id == request.BlockedDateId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        if (entity is null)
            return AppResult.Fail("not_found", "Bloqueo no encontrado.");

        _db.BlockedDates.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
