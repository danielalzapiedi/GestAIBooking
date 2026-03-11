using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestAI.Infrastructure.Saas;

public sealed class AuditService : IAuditService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;

    public AuditService(IAppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct)
    {
        var entity = new AuditLog
        {
            AccountId = accountId,
            PropertyId = propertyId,
            EntityName = entityName,
            EntityId = entityId ?? 0,
            Action = action,
            Summary = summary,
            UserId = _current.UserId,
            UserName = _current.FullName ?? _current.Email,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entity);
        await _db.SaveChangesAsync(ct);
    }
}
