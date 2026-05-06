using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class SectionConfiguration : IEntityTypeConfiguration<Section>
{
    public void Configure(EntityTypeBuilder<Section> builder)
    {
        builder.ToTable("section");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(s => s.ParentId).HasColumnName("parent_id");
        builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Position).HasColumnName("position").IsRequired();

        // SQL Server forbids ON DELETE CASCADE on a self-reference (cycle/multi-path
        // rule). DeleteSectionHandler walks descendants in code, so the DB FK only
        // needs to refuse orphans — ClientCascade keeps EF's tracked-entity cascade
        // semantics for in-memory/Sqlite tests without emitting CASCADE in DDL.
        builder.HasOne(s => s.Parent)
            .WithMany(s => s.Children)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(s => new { s.ParentId, s.Position })
            .HasDatabaseName("IX_section_parent_id_position");
    }
}
