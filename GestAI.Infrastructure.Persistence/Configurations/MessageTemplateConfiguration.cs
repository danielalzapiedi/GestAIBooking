using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
{
    public void Configure(EntityTypeBuilder<MessageTemplate> b)
    {
        b.ToTable("MessageTemplates");
        b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<int>();
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.Category).HasMaxLength(100).HasDefaultValue("General");
        b.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        b.Property(x => x.IsActive).HasDefaultValue(true);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.Property).WithMany(x => x.MessageTemplates).HasForeignKey(x => x.PropertyId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(x => new { x.PropertyId, x.Type, x.Name });
    }
}
