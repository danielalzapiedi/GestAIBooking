using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class OperationalTaskConfiguration : IEntityTypeConfiguration<OperationalTask>
{
    public void Configure(EntityTypeBuilder<OperationalTask> b)
    {
        b.ToTable("OperationalTasks");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Priority).HasConversion<int>();
        b.Property(x => x.Title).HasMaxLength(180).IsRequired();
        b.Property(x => x.ResponsibleName).HasMaxLength(180);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany(x => x.Tasks).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Unit).WithMany(x => x.Tasks).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Booking).WithMany().HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PropertyId, x.ScheduledDate, x.Status });
    }
}
