using FluentValidation;
using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Templates;

public sealed record UpsertTemplateCommand(int PropertyId, int? TemplateId, TemplateType Type, string Name, string Body, bool IsActive) : IRequest<AppResult<int>>;
public sealed record ToggleTemplateStatusCommand(int PropertyId, int TemplateId, bool IsActive) : IRequest<AppResult>;
public sealed class UpsertTemplateCommandValidator : AbstractValidator<UpsertTemplateCommand>
{
    public UpsertTemplateCommandValidator()
    {
        RuleFor(x => x.PropertyId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
public sealed class UpsertTemplateCommandHandler : IRequestHandler<UpsertTemplateCommand, AppResult<int>>
{
    private readonly IAppDbContext _db; private readonly ICurrentUser _current; private readonly IPropertyFeatureService _features;
    public UpsertTemplateCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features) { _db = db; _current = current; _features = features; }
    public async Task<AppResult<int>> Handle(UpsertTemplateCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Templates, ct))
            return AppResult<int>.Fail("feature_disabled", "Las plantillas están desactivadas para este hospedaje.");
        var propertyOk = await _db.Properties.AsNoTracking().AnyAsync(x => x.Id == request.PropertyId && (x.Account.OwnerUserId == _current.UserId || x.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (!propertyOk) return AppResult<int>.Fail("forbidden", "Propiedad inválida.");
        MessageTemplate entity;
        if (request.TemplateId is null)
        {
            entity = new MessageTemplate { PropertyId = request.PropertyId };
            _db.MessageTemplates.Add(entity);
        }
        else
        {
            entity = await _db.MessageTemplates.FirstOrDefaultAsync(x => x.Id == request.TemplateId.Value && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct)
                ?? throw new InvalidOperationException("Plantilla no encontrada.");
        }
        entity.Type = request.Type; entity.Name = request.Name.Trim(); entity.Body = request.Body.Trim(); entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return AppResult<int>.Ok(entity.Id);
    }
}

public sealed class ToggleTemplateStatusCommandHandler : IRequestHandler<ToggleTemplateStatusCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IPropertyFeatureService _features;

    public ToggleTemplateStatusCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _features = features;
    }

    public async Task<AppResult> Handle(ToggleTemplateStatusCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Templates, ct))
            return AppResult.Fail("feature_disabled", "Las plantillas están desactivadas para este hospedaje.");

        var entity = await _db.MessageTemplates
            .FirstOrDefaultAsync(x => x.Id == request.TemplateId
                && x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (entity is null)
            return AppResult.Fail("not_found", "Plantilla no encontrada.");

        entity.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
