using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> b)
    {
        b.ToTable("Properties");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CommercialName).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.City).HasMaxLength(120);
        b.Property(x => x.Province).HasMaxLength(120);
        b.Property(x => x.Country).HasMaxLength(120);
        b.Property(x => x.Address).HasMaxLength(250);
        b.Property(x => x.Currency).HasMaxLength(10).HasDefaultValue("ARS");
        b.Property(x => x.DepositPolicy).HasMaxLength(1000);
        b.Property(x => x.DefaultDepositPercentage).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
        b.Property(x => x.CancellationPolicy).HasMaxLength(2000);
        b.Property(x => x.TermsAndConditions).HasMaxLength(4000);
        b.Property(x => x.CheckInInstructions).HasMaxLength(4000);
        b.Property(x => x.PropertyRules).HasMaxLength(4000);
        b.Property(x => x.CommercialContactName).HasMaxLength(200);
        b.Property(x => x.CommercialContactPhone).HasMaxLength(100);
        b.Property(x => x.CommercialContactEmail).HasMaxLength(200);
        b.Property(x => x.PublicSlug).HasMaxLength(150);
        b.Property(x => x.PublicDescription).HasMaxLength(4000);
        b.Property(x => x.Type).HasDefaultValue(0);
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasOne(x => x.Account)
            .WithMany(x => x.Properties)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.FeatureSettings)
            .WithOne(x => x.Property)
            .HasForeignKey<PropertyFeatureSettings>(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.AccountId, x.Name }).IsUnique();
        b.HasIndex(x => x.PublicSlug).IsUnique().HasFilter("[PublicSlug] IS NOT NULL");
    }
}
