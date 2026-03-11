using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class ExternalCalendarEventConfiguration : IEntityTypeConfiguration<ExternalCalendarEvent>
{
    public void Configure(EntityTypeBuilder<ExternalCalendarEvent> b)
    {
        b.ToTable("ExternalCalendarEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalEventUid).HasMaxLength(500).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(500);
        b.Property(x => x.SourceChannel).HasConversion<int>();
        b.Property(x => x.RawData).HasMaxLength(4000);
        b.Property(x => x.SyncHash).HasMaxLength(200);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => new { x.ExternalChannelConnectionId, x.ExternalEventUid }).IsUnique();
        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.StartDate, x.EndDate });
        b.HasOne(x => x.ExternalChannelConnection).WithMany(x => x.Events).HasForeignKey(x => x.ExternalChannelConnectionId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Unit).WithMany(x => x.ExternalCalendarEvents).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Booking).WithMany().HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.SetNull);
    }
}
