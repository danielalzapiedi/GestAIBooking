using GestAI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestAI.Infrastructure.Persistence.Configurations;

public sealed class SaasPlanDefinitionConfiguration : IEntityTypeConfiguration<SaasPlanDefinition>
{
    public void Configure(EntityTypeBuilder<SaasPlanDefinition> b)
    {
        b.ToTable("SaasPlanDefinitions");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasConversion<int>();
        b.Property(x => x.Name).HasMaxLength(120).IsRequired();
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.Code).IsUnique();
    }
}
