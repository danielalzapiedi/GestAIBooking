using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> b)
    {
        b.ToTable("Guests");
        b.HasKey(x => x.Id);

        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.DocumentNumber).HasMaxLength(100);

        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Property)
            .WithMany()
            .HasForeignKey(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.PropertyId, x.FullName });
        b.HasIndex(x => new { x.PropertyId, x.Email });
        b.HasIndex(x => new { x.PropertyId, x.Phone });
    }
}
