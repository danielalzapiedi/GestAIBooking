using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Guests;

public sealed record DeleteGuestCommand(int PropertyId, int GuestId) : IRequest<AppResult>;

public sealed class DeleteGuestCommandValidator : AbstractValidator<DeleteGuestCommand>
{
    public DeleteGuestCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.GuestId).GreaterThan(0);
    }
}

public sealed class DeleteGuestCommandHandler : IRequestHandler<DeleteGuestCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public DeleteGuestCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(DeleteGuestCommand request, CancellationToken ct)
    {
        var guest = await _db.Guests
            .FirstOrDefaultAsync(g => g.Id == request.GuestId && g.PropertyId == request.PropertyId && (g.Property.Account.OwnerUserId == _current.UserId || g.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);

        if (guest is null) return AppResult.Fail("not_found", "Huésped no encontrado.");

        guest.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
