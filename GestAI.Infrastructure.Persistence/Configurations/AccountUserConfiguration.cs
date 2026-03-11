using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class AccountUserConfiguration : IEntityTypeConfiguration<AccountUser>
{
    public void Configure(EntityTypeBuilder<AccountUser> b)
    {
        b.ToTable("AccountUsers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Role).HasConversion<int>();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Account).WithMany(x => x.Users).HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.AccountId, x.UserId }).IsUnique();
    }
}
