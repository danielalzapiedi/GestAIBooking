using GestAI.Domain.Common;
using System;

namespace GestAI.Domain.Entities
{
    public class UserSubscription : Entity
    {
        public string UserId { get; set; } = default!;
        public string PayPalSubscriptionId { get; set; } = default!;
        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Basic;
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum SubscriptionPlan
    {
        None = 0,
        Basic = 1,     // USD 5
        Standard = 2,  // USD 10
        Premium = 3    // USD 15
    }

    public enum SubscriptionStatus
    {
        Pending = 0,
        Active = 1,
        Suspended = 2,
        Cancelled = 3,
        PastDue = 4
    }
}
