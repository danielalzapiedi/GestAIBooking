using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Operations;

public sealed record CompleteOperationalTaskCommand(int PropertyId, int TaskId) : IRequest<AppResult>;
public sealed record UpdateOperationalTaskStatusCommand(int PropertyId, int TaskId, OperationalTaskStatus Status) : IRequest<AppResult>;

public sealed class CompleteOperationalTaskCommandHandler : IRequestHandler<CompleteOperationalTaskCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;

    public CompleteOperationalTaskCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features, IUserAccessService access)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
    }

    public async Task<AppResult> Handle(CompleteOperationalTaskCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Housekeeping, ct))
            return AppResult.Fail("feature_disabled", "Housekeeping está desactivado para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Housekeeping, ct))
            return AppResult.Fail("forbidden", "No tenés acceso al módulo de housekeeping.");

        var task = await _db.OperationalTasks.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == request.TaskId && x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (task is null)
            return AppResult.Fail("not_found", "Tarea no encontrada.");

        task.Status = OperationalTaskStatus.Completed;
        task.CompletedAtUtc = DateTime.UtcNow;

        if (task.Type == OperationalTaskType.Cleaning && task.Unit is not null)
            task.Unit.OperationalStatus = UnitOperationalStatus.Clean;

        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}

public sealed class UpdateOperationalTaskStatusCommandHandler : IRequestHandler<UpdateOperationalTaskStatusCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;

    public UpdateOperationalTaskStatusCommandHandler(IAppDbContext db, ICurrentUser current, IPropertyFeatureService features, IUserAccessService access)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
    }

    public async Task<AppResult> Handle(UpdateOperationalTaskStatusCommand request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Housekeeping, ct))
            return AppResult.Fail("feature_disabled", "Housekeeping está desactivado para este hospedaje.");

        if (!await _access.HasPropertyModuleAccessAsync(request.PropertyId, SaasModule.Housekeeping, ct))
            return AppResult.Fail("forbidden", "No tenés acceso al módulo de housekeeping.");

        var task = await _db.OperationalTasks.Include(x => x.Unit)
            .FirstOrDefaultAsync(x => x.Id == request.TaskId
                && x.PropertyId == request.PropertyId
                && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)), ct);
        if (task is null)
            return AppResult.Fail("not_found", "Tarea no encontrada.");

        task.Status = request.Status;
        task.CompletedAtUtc = request.Status == OperationalTaskStatus.Completed ? DateTime.UtcNow : null;

        if (task.Type == OperationalTaskType.Cleaning && task.Unit is not null)
        {
            task.Unit.OperationalStatus = request.Status switch
            {
                OperationalTaskStatus.InProgress => UnitOperationalStatus.Cleaning,
                OperationalTaskStatus.Completed => UnitOperationalStatus.Clean,
                _ => UnitOperationalStatus.PendingCleaning
            };
        }

        await _db.SaveChangesAsync(ct);
        return AppResult.Ok();
    }
}
