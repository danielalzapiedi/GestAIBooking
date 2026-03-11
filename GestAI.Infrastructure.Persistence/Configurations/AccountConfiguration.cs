using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("Accounts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);

        b.Property(x => x.RowVersion).IsRowVersion();

        b.HasIndex(x => new { x.OwnerUserId, x.Name }).IsUnique();
    }
}
