using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Operations;

public sealed record CompleteOperationalTaskCommand(int PropertyId, int TaskId) : IRequest<AppResult>;

public sealed class CompleteOperationalTaskCommandHandler : IRequestHandler<CompleteOperationalTaskCommand, AppResult>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;

    public CompleteOperationalTaskCommandHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, IPropertyFeatureService features)
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

        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);
        if (property is null)
            return AppResult.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Housekeeping, ct))
            return AppResult.Fail("forbidden", "No tenés permisos para usar el módulo de housekeeping.");

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
