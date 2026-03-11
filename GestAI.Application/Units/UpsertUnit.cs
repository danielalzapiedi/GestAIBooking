using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Units;

public sealed record UpsertUnitCommand(int PropertyId, int? UnitId, string Name, int CapacityAdults, int CapacityChildren, decimal BaseRate, int TotalCapacity, string? ShortDescription, bool IsActive, int DisplayOrder, UnitOperationalStatus OperationalStatus) : IRequest<AppResult<int>>;
public sealed record SetUnitOperationalStatusCommand(int PropertyId, int UnitId, UnitOperationalStatus Status) : IRequest<AppResult>;

public sealed class UpsertUnitCommandValidator : AbstractValidator<UpsertUnitCommand>
{
    public UpsertUnitCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.CapacityAdults).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CapacityChildren).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalCapacity).GreaterThan(0);
        RuleFor(x => x.BaseRate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TotalCapacity).GreaterThanOrEqualTo(x => x.CapacityAdults + x.CapacityChildren)
            .WithMessage("La capacidad total no puede ser menor a la suma de adultos y niños.");
    }
}

public sealed class UpsertUnitCommandHandler : IRequestHandler<UpsertUnitCommand, AppResult<int>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly ISaasPlanService _plan;
    private readonly IAuditService _audit;

    public UpsertUnitCommandHandler(IAppDbContext db, ICurrentUser current, ISaasPlanService plan, IAuditService audit)
    {
        _db = db;
        _current = current;
        _plan = plan;
        _audit = audit;
    }

    public async Task<AppResult<int>> Handle(UpsertUnitCommand request, CancellationToken ct)
    {
        var property = await _db.Properties.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (property is null) return AppResult<int>.Fail("forbidden", "Propiedad inválida.");
        if (!property.IsActive && request.IsActive)
            return AppResult<int>.Fail("property_inactive", "No podés activar una unidad en un hospedaje inactivo.");

        if (request.UnitId is null)
        {
            var limit = await _plan.ValidateUnitCreationAsync(property.AccountId, ct);
            if (!limit.Success) return AppResult<int>.Fail(limit.ErrorCode!, limit.Message!);
        }

        var duplicateName = await _db.Units.AsNoTracking()
            .AnyAsync(x => x.PropertyId == request.PropertyId && x.Id != (request.UnitId ?? 0) && x.Name == request.Name.Trim(), ct);
        if (duplicateName) return AppResult<int>.Fail("duplicate_name", "Ya existe una unidad con ese nombre en el hospedaje.");

        GestAI.Domain.Entities.Unit entity;
        if (request.UnitId is null)
        {
            entity = new GestAI.Domain.Entities.Unit { PropertyId = request.PropertyId };
            _db.Units.Add(entity);
        }
        else
        {
            entity = await _db.Units.FirstOrDefaultAsync(x => x.Id == request.UnitId.Value && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Unidad no encontrada.");
        }
        entity.Name = request.Name.Trim();
        entity.CapacityAdults = request.CapacityAdults;
        entity.CapacityChildren = request.CapacityChildren;
        entity.BaseRate = request.BaseRate;
        entity.TotalCapacity = request.TotalCapacity;
        entity.ShortDescription = request.ShortDescription?.Trim();
        entity.IsActive = request.IsActive;
        entity.DisplayOrder = request.DisplayOrder;
        entity.OperationalStatus = request.IsActive ? request.OperationalStatus : UnitOperationalStatus.Maintenance;
        await _db.SaveChangesAsync(ct);
        await _audit.WriteAsync(property.AccountId, property.Id, "Unit", entity.Id, request.UnitId is null ? "created" : "updated", $"Unidad {(request.UnitId is null ? "creada" : "actualizada")}: {entity.Name}", ct);
        return AppResult<int>.Ok(entity.Id);
    }
}

public sealed class SetUnitOperationalStatusCommandHandler : IRequestHandler<SetUnitOperationalStatusCommand, AppResult>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current;
    public SetUnitOperationalStatusCommandHandler(IAppDbContext db, ICurrentUser current) { _db = db; _current = current; }
    public async Task<AppResult> Handle(SetUnitOperationalStatusCommand request, CancellationToken ct)
    {
        var unit = await _db.Units.FirstOrDefaultAsync(x => x.Id == request.UnitId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (unit is null) return AppResult.Fail("not_found", "Unidad no encontrada.");
        if (!unit.IsActive) return AppResult.Fail("inactive", "No podés cambiar el estado operativo de una unidad inactiva.");
        unit.OperationalStatus = request.Status;
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
