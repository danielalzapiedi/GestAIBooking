using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> b)
    {
        b.ToTable("Units");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.ShortDescription).HasMaxLength(500);
        b.Property(x => x.BaseRate).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.TotalCapacity).HasDefaultValue(2);
        b.Property(x => x.DisplayOrder).HasDefaultValue(0);
        b.Property(x => x.OperationalStatus).HasConversion<int>().HasDefaultValue(UnitOperationalStatus.Available);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Property)
            .WithMany(x => x.Units)
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.PropertyId, x.Name }).IsUnique();
    }
}
