using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class AuditLog : Entity
{
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public int? PropertyId { get; set; }
    public Property? Property { get; set; }
    public string EntityName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public int EntityId { get; set; }
    public string? Summary { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
