using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class ExternalSyncLogConfiguration : IEntityTypeConfiguration<ExternalSyncLog>
{
    public void Configure(EntityTypeBuilder<ExternalSyncLog> b)
    {
        b.ToTable("ExternalSyncLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Message).HasMaxLength(2000);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.ExternalChannelConnectionId, x.CreatedAtUtc });
        b.HasOne(x => x.ExternalChannelConnection).WithMany(x => x.SyncLogs).HasForeignKey(x => x.ExternalChannelConnectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
    }
}
