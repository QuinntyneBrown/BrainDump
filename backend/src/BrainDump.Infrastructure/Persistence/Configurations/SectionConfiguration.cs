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
        builder.Property(s => s.DocumentId).HasColumnName("document_id").IsRequired();
        builder.Property(s => s.ParentId).HasColumnName("parent_id");
        builder.Property(s => s.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(s => s.Position).HasColumnName("position").IsRequired();

        // section→document is the cascading edge: deleting a document drops
        // every section (and via cascade on fact, every fact) that belongs
        // to it. ClientCascade keeps EF's tracked-entity cascade semantics
        // without emitting CASCADE in DDL.
        builder.HasOne(s => s.Document)
            .WithMany(d => d.Sections)
            .HasForeignKey(s => s.DocumentId)
            .OnDelete(DeleteBehavior.ClientCascade);

        // Self-reference uses ClientCascade for the same SQL-Server cycle
        // reason as task 02 design §3.
        builder.HasOne(s => s.Parent)
            .WithMany(s => s.Children)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(s => new { s.DocumentId, s.ParentId, s.Position })
            .HasDatabaseName("IX_section_document_id_parent_id_position");
    }
}
