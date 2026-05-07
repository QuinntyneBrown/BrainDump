using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class DocumentLinkConfiguration : IEntityTypeConfiguration<DocumentLink>
{
    public void Configure(EntityTypeBuilder<DocumentLink> builder)
    {
        builder.ToTable("document_link");
        builder.HasKey(dl => new { dl.SourceDocumentId, dl.TargetDocumentId });
        builder.Property(dl => dl.SourceDocumentId).HasColumnName("source_document_id");
        builder.Property(dl => dl.TargetDocumentId).HasColumnName("target_document_id");

        // SQL Server forbids two cascading FKs to the same table from the
        // same row when both could fire in a single delete. Use Cascade on
        // the source side and ClientCascade on the target side; the latter
        // means EF cascades target deletes when the row is tracked, while
        // the design's expected cleanup paths (delete a document) only
        // touch the source side via the database.
        builder.HasOne(dl => dl.Source)
            .WithMany()
            .HasForeignKey(dl => dl.SourceDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dl => dl.Target)
            .WithMany()
            .HasForeignKey(dl => dl.TargetDocumentId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(dl => dl.TargetDocumentId)
            .HasDatabaseName("IX_document_link_target_document_id");
    }
}
