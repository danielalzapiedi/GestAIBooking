using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class AccountSubscriptionPlanConfiguration : IEntityTypeConfiguration<AccountSubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<AccountSubscriptionPlan> b)
    {
        b.ToTable("AccountSubscriptionPlans");
        b.HasKey(x => x.Id);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Account).WithMany(x => x.SubscriptionPlans).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.PlanDefinition).WithMany().HasForeignKey(x => x.PlanDefinitionId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.AccountId, x.IsActive });
    }
}
