using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Promotions;

public sealed class UpsertPromotionCommandValidator : AbstractValidator<UpsertPromotionCommand>
{
    public UpsertPromotionCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Value).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Priority).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DateTo).GreaterThanOrEqualTo(x => x.DateFrom);
    }
}

public sealed class PromotionCommandsHandler :
    IRequestHandler<UpsertPromotionCommand, AppResult<int>>,
    IRequestHandler<TogglePromotionStatusCommand, AppResult>,
    IRequestHandler<DeletePromotionCommand, AppResult>,
    IRequestHandler<GetPromotionsQuery, AppResult<List<PromotionDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;
    private readonly IUserAccessService _access;

    public PromotionCommandsHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features, IUserAccessService access)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
        _access = access;
    }

    public async Task<AppResult<int>> Handle(UpsertPromotionCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Promotions, ct))
            return AppResult<int>.Fail("feature_disabled", "Las promociones están desactivadas para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Promotions, ct))
            return AppResult<int>.Fail("forbidden", "No tenés acceso al módulo de promociones.");

        var property = await _db.Properties.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (property is null)
            return AppResult<int>.Fail("forbidden", "Hospedaje inválido.");
        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Promotions, ct))
            return AppResult<int>.Fail("forbidden", "No tenés permisos para usar el módulo de promociones.");
        if (!property.IsActive && request.IsActive)
            return AppResult<int>.Fail("property_inactive", "No podés activar promociones en un hospedaje inactivo.");

        if (request.UnitId.HasValue)
        {
            var unit = await _db.Units.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.UnitId.Value && x.PropertyId == request.PropertyId, ct);
            if (unit is null)
                return AppResult<int>.Fail("unit_invalid", "Unidad inválida.");
            if (!unit.IsActive && request.IsActive)
                return AppResult<int>.Fail("unit_inactive", "No podés activar promociones en una unidad inactiva.");
        }

        Promotion entity;
        if (request.PromotionId is null)
        {
            entity = new Promotion { PropertyId = request.PropertyId };
            _db.Promotions.Add(entity);
        }
        else
        {
            entity = await _db.Promotions.FirstOrDefaultAsync(x => x.Id == request.PromotionId.Value && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Promoción no encontrada.");
        }

        entity.UnitId = request.UnitId;
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.IsActive = request.IsActive;
        entity.ValueType = request.ValueType;
        entity.Scope = request.Scope;
        entity.Value = request.Value;
        entity.IsCumulative = request.IsCumulative;
        entity.Priority = request.Priority;
        entity.DateFrom = request.DateFrom;
        entity.DateTo = request.DateTo;
        entity.MinNights = request.MinNights;
        entity.MaxNights = request.MaxNights;
        entity.BookingWindowDaysMin = request.BookingWindowDaysMin;
        entity.BookingWindowDaysMax = request.BookingWindowDaysMax;
        entity.AllowedCheckInDays = request.AllowedCheckInDays;
        entity.AllowedCheckOutDays = request.AllowedCheckOutDays;
        entity.IsDeleted = false;

        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(entity.Id);
    }

    public async Task<AppResult> Handle(TogglePromotionStatusCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Promotions, ct))
            return AppResult.Fail("feature_disabled", "Las promociones están desactivadas para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Promotions, ct))
            return AppResult.Fail("forbidden", "No tenés acceso al módulo de promociones.");

        var entity = await _db.Promotions.FirstOrDefaultAsync(x => x.Id == request.PromotionId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (entity is null)
            return AppResult.Fail("not_found", "Promoción no encontrada.");
        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }

    public async Task<AppResult> Handle(DeletePromotionCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Promotions, ct))
            return AppResult.Fail("feature_disabled", "Las promociones están desactivadas para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Promotions, ct))
            return AppResult.Fail("forbidden", "No tenés acceso al módulo de promociones.");

        var entity = await _db.Promotions.FirstOrDefaultAsync(x => x.Id == request.PromotionId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (entity is null)
            return AppResult.Fail("not_found", "Promoción no encontrada.");
        entity.IsDeleted = true;
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }

    public async Task<AppResult<List<PromotionDto>>> Handle(GetPromotionsQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Promotions, ct))
            return AppResult<List<PromotionDto>>.Fail("feature_disabled", "Las promociones están desactivadas para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Promotions, ct))
            return AppResult<List<PromotionDto>>.Fail("forbidden", "No tenés acceso al módulo de promociones.");

        var data = await _db.Promotions.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)) && (request.IncludeDeleted || !x.IsDeleted))
            .Include(x => x.Unit)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .Select(x => new PromotionDto(
                x.Id,
                x.PropertyId,
                x.UnitId,
                x.Unit != null ? x.Unit.Name : null,
                x.Name,
                x.Description,
                x.IsActive,
                x.IsDeleted,
                x.ValueType,
                x.Scope,
                x.Value,
                x.IsCumulative,
                x.Priority,
                x.DateFrom,
                x.DateTo,
                x.MinNights,
                x.MaxNights,
                x.BookingWindowDaysMin,
                x.BookingWindowDaysMax,
                x.AllowedCheckInDays,
                x.AllowedCheckOutDays))
            .ToListAsync(ct);

        return AppResult<List<PromotionDto>>.Ok(data);
    }
}
