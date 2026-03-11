using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class AccountSubscriptionPlan : Entity
{
    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public int PlanDefinitionId { get; set; }
    public SaasPlanDefinition PlanDefinition { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime StartedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ChangedAtUtc { get; set; }
}
