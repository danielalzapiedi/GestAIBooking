using GestAI.Application.Abstractions;
using GestAI.Application.Common;
using GestAI.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Application.Operations;

public sealed record GetOperationalTasksQuery(int PropertyId, OperationalTaskStatus? Status = null, OperationalTaskType? Type = null) : IRequest<AppResult<List<OperationalTaskDto>>>;

public sealed class GetOperationalTasksQueryHandler : IRequestHandler<GetOperationalTasksQuery, AppResult<List<OperationalTaskDto>>>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly IUserAccessService _access;
    private readonly IPropertyFeatureService _features;

    public GetOperationalTasksQueryHandler(IAppDbContext db, ICurrentUser current, IUserAccessService access, IPropertyFeatureService features)
    {
        _db = db;
        _current = current;
        _access = access;
        _features = features;
    }

    public async Task<AppResult<List<OperationalTaskDto>>> Handle(GetOperationalTasksQuery request, CancellationToken ct)
    {
        if (!await _features.IsEnabledAsync(request.PropertyId, PropertyFeature.Housekeeping, ct))
            return AppResult<List<OperationalTaskDto>>.Fail("feature_disabled", "Housekeeping está desactivado para este hospedaje.");

        var property = await PropertyAuthorization.GetAccessiblePropertyAsync(_db, _current, request.PropertyId, ct);
        if (property is null)
            return AppResult<List<OperationalTaskDto>>.Fail("forbidden", "Hospedaje inválido o sin acceso.");

        if (!await _access.HasModuleAccessAsync(property.AccountId, SaasModule.Housekeeping, ct))
            return AppResult<List<OperationalTaskDto>>.Fail("forbidden", "No tenés permisos para usar el módulo de housekeeping.");

        var query = _db.OperationalTasks.AsNoTracking()
            .Where(x => x.PropertyId == request.PropertyId && (x.Property.Account.OwnerUserId == _current.UserId || x.Property.Account.Users.Any(au => au.UserId == _current.UserId && au.IsActive)));

        if (request.Status.HasValue)
            query = query.Where(x => x.Status == request.Status.Value);
        if (request.Type.HasValue)
            query = query.Where(x => x.Type == request.Type.Value);

        var data = await query
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.Priority)
            .ThenBy(x => x.ScheduledDate)
            .Select(x => new OperationalTaskDto(
                x.Id,
                x.PropertyId,
                x.UnitId,
                x.Unit != null ? x.Unit.Name : null,
                x.BookingId,
                x.Booking != null ? x.Booking.BookingCode : null,
                x.Type,
                x.Status,
                x.Priority,
                x.ScheduledDate,
                x.Title,
                x.Notes,
                x.CreatedAtUtc,
                x.CompletedAtUtc))
            .ToListAsync(ct);

        return AppResult<List<OperationalTaskDto>>.Ok(data);
    }
}
