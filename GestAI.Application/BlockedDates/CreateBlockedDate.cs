using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.BlockedDates;

public sealed record CreateBlockedDateCommand(int PropertyId, int UnitId, DateOnly DateFrom, DateOnly DateTo, string? Reason) : IRequest<AppResult<int>>;

public sealed class CreateBlockedDateCommandValidator : AbstractValidator<CreateBlockedDateCommand>
{
    public CreateBlockedDateCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.DateTo).GreaterThan(x => x.DateFrom);
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

public sealed class CreateBlockedDateCommandHandler : IRequestHandler<CreateBlockedDateCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public CreateBlockedDateCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult<int>> Handle(CreateBlockedDateCommand request, CancellationToken ct)
    {
        var unitOk = await _db.Units.AsNoTracking()
            .AnyAsync(u => u.Id == request.UnitId && u.PropertyId == request.PropertyId && (u.Property.Account.OwnerUserId == _current.UserId || u.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && u.IsActive, ct);
        if (!unitOk) return AppResult<int>.Fail("forbidden", "Unidad inválida o sin acceso.");

        // No solapa con reservas Tentative/Confirmed
        var overlapBooking = await _db.Bookings.AsNoTracking()
            .Where(b => b.PropertyId == request.PropertyId && b.UnitId == request.UnitId)
            .Where(b => b.Status != BookingStatus.Cancelled)
            .AnyAsync(b => b.CheckInDate < request.DateTo && request.DateFrom < b.CheckOutDate, ct);

        if (overlapBooking)
            return AppResult<int>.Fail("overlap", "El bloqueo se solapa con una reserva existente.");

        // No solapa con otro bloqueo
        var overlapBlocked = await _db.BlockedDates.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && x.UnitId == request.UnitId)
            .AnyAsync(x => x.DateFrom < request.DateTo && request.DateFrom < x.DateTo, ct);

        if (overlapBlocked)
            return AppResult<int>.Fail("overlap", "Ya existe un bloqueo en ese rango.");

        var entity = new BlockedDate
        {
            PropertyId = request.PropertyId,
            UnitId = request.UnitId,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            Reason = request.Reason?.Trim()
        };

        _db.BlockedDates.Add(entity);
        await _db.SaveChangesAsync(ct);

        return AppResult<int>.Ok(entity.Id);
    }
}
