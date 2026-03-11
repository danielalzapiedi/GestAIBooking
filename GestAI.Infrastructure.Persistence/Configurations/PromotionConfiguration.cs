using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> b)
    {
        b.ToTable("Promotions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(160).IsRequired();
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.ValueType).HasConversion<int>();
        b.Property(x => x.Scope).HasConversion<int>();
        b.Property(x => x.Value).HasColumnType("decimal(18,2)");
        b.Property(x => x.AllowedCheckInDays).HasMaxLength(80);
        b.Property(x => x.AllowedCheckOutDays).HasMaxLength(80);
        b.Property(x => x.Priority).HasDefaultValue(100);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany(x => x.Promotions).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Unit).WithMany(x => x.Promotions).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.IsActive, x.IsDeleted, x.DateFrom, x.DateTo });
    }
}
