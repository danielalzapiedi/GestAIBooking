using GestAI.Domain.Common;

namespace GestAI.Domain.Entities;

public sealed class Account : Entity
{
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string OwnerUserId { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ICollection<Property> Properties { get; set; } = new List<Property>();
    public ICollection<AccountUser> Users { get; set; } = new List<AccountUser>();
    public ICollection<AccountSubscriptionPlan> SubscriptionPlans { get; set; } = new List<AccountSubscriptionPlan>();
}
