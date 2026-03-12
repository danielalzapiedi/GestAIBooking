using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class PropertyFeatureSettingsConfiguration : IEntityTypeConfiguration<PropertyFeatureSettings>
{
    public void Configure(EntityTypeBuilder<PropertyFeatureSettings> b)
    {
        b.ToTable("PropertyFeatureSettings");
        b.HasKey(x => x.Id);

        b.Property(x => x.EnableHousekeeping).HasDefaultValue(true);
        b.Property(x => x.EnableAgenda).HasDefaultValue(true);
        b.Property(x => x.EnableQuotes).HasDefaultValue(true);
        b.Property(x => x.EnableSavedQuotes).HasDefaultValue(true);
        b.Property(x => x.EnablePromotions).HasDefaultValue(true);
        b.Property(x => x.EnableAdvancedRates).HasDefaultValue(true);
        b.Property(x => x.EnablePayments).HasDefaultValue(true);
        b.Property(x => x.EnableDirectBooking).HasDefaultValue(true);
        b.Property(x => x.EnableExternalCalendarSync).HasDefaultValue(true);
        b.Property(x => x.EnableReports).HasDefaultValue(true);
        b.Property(x => x.EnableTemplates).HasDefaultValue(true);
        b.Property(x => x.EnableAuditView).HasDefaultValue(true);
        b.Property(x => x.UseSimpleGuestMode).HasDefaultValue(false);

        b.HasOne(x => x.Property)
            .WithOne(x => x.FeatureSettings)
            .HasForeignKey<PropertyFeatureSettings>(x => x.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.PropertyId).IsUnique();
    }
}
