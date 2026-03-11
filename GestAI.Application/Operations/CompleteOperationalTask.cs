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

    public CompleteOperationalTaskCommandHandler(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<AppResult> Handle(CompleteOperationalTaskCommand request, CancellationToken ct)
    {
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
