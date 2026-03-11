using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);
        b.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Action).HasMaxLength(100).IsRequired();
        b.Property(x => x.Summary).HasMaxLength(2000);
        b.Property(x => x.UserId).HasMaxLength(450);
        b.Property(x => x.UserName).HasMaxLength(200);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Property).WithMany().HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.PropertyId, x.CreatedAtUtc });
    }
}
