using GestAI.Application.Abstractions;
using GestAI.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace GestAI.Infrastructure.Saas;

public sealed class AuditService : IAuditService
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _current;
    private readonly ILogger<AuditService> _logger;

    public AuditService(IAppDbContext db, ICurrentUser current, ILogger<AuditService> logger)
    {
        _db = db;
        _current = current;
        _logger = logger;
    }

    public async Task WriteAsync(int accountId, int? propertyId, string entityName, int? entityId, string action, string summary, CancellationToken ct)
    {
        var (userId, userName) = ResolveAuditActor();

        var entity = new AuditLog
        {
            AccountId = accountId,
            PropertyId = propertyId,
            EntityName = entityName,
            EntityId = entityId ?? 0,
            Action = action,
            Summary = summary,
            UserId = userId,
            UserName = userName,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.AuditLogs.Add(entity);
        await _db.SaveChangesAsync(ct);
    }

    private (string? UserId, string UserName) ResolveAuditActor()
    {
        try
        {
            var userId = _current.UserId;
            var userName = _current.FullName ?? _current.Email ?? "SYSTEM";
            return (string.IsNullOrWhiteSpace(userId) ? null : userId, userName);
        }
        catch (InvalidOperationException)
        {
            _logger.LogInformation("AUDIT: Action executed by SYSTEM.");
            return (null, "SYSTEM");
        }
    }
}
