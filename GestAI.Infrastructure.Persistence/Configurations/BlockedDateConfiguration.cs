using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class BlockedDateConfiguration : IEntityTypeConfiguration<BlockedDate>
{
    public void Configure(EntityTypeBuilder<BlockedDate> b)
    {
        b.ToTable("BlockedDates");
        b.HasKey(x => x.Id);

        b.Property(x => x.Reason).HasMaxLength(500);
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Unit)
            .WithMany(x => x.BlockedDates)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.DateFrom, x.DateTo });
    }
}
