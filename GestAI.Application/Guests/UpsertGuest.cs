using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Guests;

public sealed record UpsertGuestCommand(
    int PropertyId,
    int? GuestId,
    string FullName,
    string? Phone,
    string? Email,
    int? DocumentType,
    string? DocumentNumber,
    string? Notes
) : IRequest<AppResult<int>>;

public sealed class UpsertGuestCommandValidator : AbstractValidator<UpsertGuestCommand>
{
    public UpsertGuestCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Email).MaximumLength(200);
        RuleFor(x => x.DocumentNumber).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public sealed class UpsertGuestCommandHandler : IRequestHandler<UpsertGuestCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public UpsertGuestCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<int>> Handle(UpsertGuestCommand request, CancellationToken ct)
    {
        var hasAccess = await _db.Properties.AsNoTracking()
            .AnyAsync(p => p.Id == request.PropertyId && (p.Account.OwnerUserId == _current.UserId || p.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && p.IsActive, ct);
        if (!hasAccess) return AppResult<int>.Fail("forbidden", "Sin acceso al hospedaje.");

        Guest entity;
        if (request.GuestId is null)
        {
            entity = new Guest { PropertyId = request.PropertyId };
            _db.Guests.Add(entity);
        }
        else
        {
            entity = await _db.Guests.FirstOrDefaultAsync(g =>
                g.Id == request.GuestId.Value &&
                g.PropertyId == request.PropertyId &&
                (g.Property.Account.OwnerUserId == _current.UserId || g.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct) ?? throw new InvalidOperationException("Guest no encontrado.");
        }

        entity.FullName = request.FullName.Trim();
        entity.Phone = request.Phone?.Trim();
        entity.Email = request.Email?.Trim();
        entity.DocumentType = request.DocumentType;
        entity.DocumentNumber = request.DocumentNumber?.Trim();
        entity.Notes = request.Notes?.Trim();
        entity.IsActive = true;

        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(entity.Id);
    }
}
