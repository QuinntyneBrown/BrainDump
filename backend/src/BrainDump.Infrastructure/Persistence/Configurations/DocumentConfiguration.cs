using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("document");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(d => d.FolderId).HasColumnName("folder_id");
        builder.Property(d => d.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(d => d.Position).HasColumnName("position").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at").IsRequired();

        // ClientCascade keeps EF cascading semantics without emitting
        // CASCADE in DDL — folder→document deletion is driven by the
        // DeleteFolder handler walking descendants in code.
        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(d => new { d.FolderId, d.Position })
            .HasDatabaseName("IX_document_folder_id_position");
    }
}
