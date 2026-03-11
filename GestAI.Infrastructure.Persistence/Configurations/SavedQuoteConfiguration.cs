using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class SavedQuoteConfiguration : IEntityTypeConfiguration<SavedQuote>
{
    public void Configure(EntityTypeBuilder<SavedQuote> b)
    {
        b.ToTable("SavedQuotes");
        b.HasKey(x => x.Id);
        b.Property(x => x.PublicToken).HasMaxLength(80).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(4000);
        b.Property(x => x.AppliedPromotionNames).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.GuestName).HasMaxLength(200);
        b.Property(x => x.GuestEmail).HasMaxLength(200);
        b.Property(x => x.GuestPhone).HasMaxLength(100);
        b.Property(x => x.BaseAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.PromotionsAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.SuggestedDepositAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedBooking).WithMany().HasForeignKey(x => x.CreatedBookingId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.PublicToken).IsUnique();
        b.HasIndex(x => new { x.PropertyId, x.Status, x.CreatedAtUtc });
    }
}
