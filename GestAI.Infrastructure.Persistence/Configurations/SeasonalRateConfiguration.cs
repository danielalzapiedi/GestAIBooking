using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class SeasonalRateConfiguration : IEntityTypeConfiguration<SeasonalRate>
{
    public void Configure(EntityTypeBuilder<SeasonalRate> b)
    {
        b.ToTable("SeasonalRates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.AdjustmentType).HasConversion<int>();
        b.Property(x => x.AdjustmentValue).HasColumnType("decimal(18,2)");
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.RatePlan).WithMany(x => x.SeasonalRates).HasForeignKey(x => x.RatePlanId).OnDelete(DeleteBehavior.Cascade);
    }
}
