using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("Payments");
        b.HasKey(x => x.Id);

        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Method).HasConversion<int>().HasDefaultValue(PaymentMethod.Cash);
        b.Property(x => x.Status).HasConversion<int>().HasDefaultValue(PaymentStatus.Paid);

        b.Property(x => x.Notes).HasMaxLength(1000);

        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Booking)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.PropertyId, x.BookingId, x.Date });
    }
}
