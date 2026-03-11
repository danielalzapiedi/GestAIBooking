using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class ExternalChannelConnectionConfiguration : IEntityTypeConfiguration<ExternalChannelConnection>
{
    public void Configure(EntityTypeBuilder<ExternalChannelConnection> b)
    {
        b.ToTable("ExternalChannelConnections");
        b.HasKey(x => x.Id);
        b.Property(x => x.ChannelType).HasConversion<int>();
        b.Property(x => x.LastSyncStatus).HasConversion<int>();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.ImportCalendarUrl).HasMaxLength(2000).IsRequired();
        b.Property(x => x.ExportToken).HasMaxLength(120).IsRequired();
        b.Property(x => x.LastSyncMessage).HasMaxLength(2000);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.ExportToken).IsUnique();
        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.ChannelType, x.ImportCalendarUrl }).IsUnique();
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Unit).WithMany(x => x.ExternalChannelConnections).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
