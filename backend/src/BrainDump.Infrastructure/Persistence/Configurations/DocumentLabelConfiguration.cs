using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class DocumentLabelConfiguration : IEntityTypeConfiguration<DocumentLabel>
{
    public void Configure(EntityTypeBuilder<DocumentLabel> builder)
    {
        builder.ToTable("document_label");
        builder.HasKey(dl => new { dl.DocumentId, dl.LabelId });
        builder.Property(dl => dl.DocumentId).HasColumnName("document_id");
        builder.Property(dl => dl.LabelId).HasColumnName("label_id");

        builder.HasOne(dl => dl.Document)
            .WithMany()
            .HasForeignKey(dl => dl.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(dl => dl.Label)
            .WithMany()
            .HasForeignKey(dl => dl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dl => dl.LabelId)
            .HasDatabaseName("IX_document_label_label_id");
    }
}
