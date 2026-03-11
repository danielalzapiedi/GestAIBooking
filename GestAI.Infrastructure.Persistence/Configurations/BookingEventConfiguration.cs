using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class BookingEventConfiguration : IEntityTypeConfiguration<BookingEvent>
{
    public void Configure(EntityTypeBuilder<BookingEvent> b)
    {
        b.ToTable("BookingEvents");
        b.HasKey(x => x.Id);
        b.Property(x => x.EventType).HasConversion<int>();
        b.Property(x => x.Title).HasMaxLength(180).IsRequired();
        b.Property(x => x.Detail).HasMaxLength(2000);
        b.Property(x => x.ChangedByUserId).HasMaxLength(450);
        b.Property(x => x.ChangedByName).HasMaxLength(200);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.NoAction);
        b.HasOne(x => x.Booking).WithMany(x => x.Events).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.NoAction);
        b.HasIndex(x => new { x.BookingId, x.ChangedAtUtc });
    }
}
