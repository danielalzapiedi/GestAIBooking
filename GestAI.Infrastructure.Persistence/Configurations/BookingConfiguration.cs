using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> b)
    {
        b.ToTable("Bookings");
        b.HasKey(x => x.Id);

        b.Property(x => x.BookingCode).HasMaxLength(30).IsRequired();
        b.Property(x => x.Status).HasConversion<int>().HasDefaultValue(BookingStatus.Tentative);
        b.Property(x => x.Source).HasConversion<int>().HasDefaultValue(BookingSource.Direct);
        b.Property(x => x.OperationalStatus).HasConversion<int>().HasDefaultValue(BookingOperationalStatus.PendingCheckIn);
        b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.BaseAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.PromotionsAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.ExpectedDepositAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.SuggestedNightlyRate).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.ManualPriceOverride).HasDefaultValue(false);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.InternalNotes).HasMaxLength(2000);
        b.Property(x => x.GuestVisibleNotes).HasMaxLength(2000);
        b.Property(x => x.Tags).HasMaxLength(2000);
        b.Property(x => x.CheckOutNotes).HasMaxLength(2000);
        b.Property(x => x.CheckInNotes).HasMaxLength(2000);
        b.Property(x => x.CancellationReason).HasMaxLength(2000);
        b.Property(x => x.CancellationPolicyApplied).HasMaxLength(2000);
        b.Property(x => x.AppliedPromotionNames).HasMaxLength(2000);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Unit).WithMany(x => x.Bookings).HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Guest).WithMany(x => x.Bookings).HasForeignKey(x => x.GuestId).OnDelete(DeleteBehavior.NoAction);

        b.HasIndex(x => x.BookingCode).IsUnique();
        b.HasIndex(x => new { x.PropertyId, x.UnitId, x.CheckInDate, x.CheckOutDate });
    }
}
