using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class RatePlanConfiguration : IEntityTypeConfiguration<RatePlan>
{
    public void Configure(EntityTypeBuilder<RatePlan> b)
    {
        b.ToTable("RatePlans");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.BaseNightlyRate).HasColumnType("decimal(18,2)");
        b.Property(x => x.WeekendAdjustmentValue).HasColumnType("decimal(18,2)");
        b.Property(x => x.WeekendAdjustmentType).HasConversion<int>();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Unit).WithMany(x => x.RatePlans).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.Name }).IsUnique();
    }
}
