using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class UserDocumentViewConfiguration : IEntityTypeConfiguration<UserDocumentView>
{
    public void Configure(EntityTypeBuilder<UserDocumentView> builder)
    {
        builder.ToTable("user_document_view");
        builder.HasKey(v => new { v.UserId, v.DocumentId });
        builder.Property(v => v.UserId).HasColumnName("user_id");
        builder.Property(v => v.DocumentId).HasColumnName("document_id");
        builder.Property(v => v.ViewedAt).HasColumnName("viewed_at").IsRequired();

        builder.HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Document)
            .WithMany()
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Drives the GET /api/recent query: per user, ordered by recency.
        builder.HasIndex(v => new { v.UserId, v.ViewedAt })
            .IsDescending(false, true)
            .HasDatabaseName("IX_user_document_view_user_id_viewed_at_desc");
    }
}
