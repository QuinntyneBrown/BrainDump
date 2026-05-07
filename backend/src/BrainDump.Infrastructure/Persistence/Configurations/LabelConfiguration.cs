using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable("label");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id").UseIdentityColumn();
        // SQL Server default collation is case-insensitive, so the unique
        // index naturally enforces case-insensitive uniqueness on `name`.
        // Sqlite (used in integration tests) defaults to case-sensitive
        // collation; the SetDocumentLabels handler normalizes input via
        // lower-cased lookup so duplicates can't slip through there either.
        builder.Property(l => l.Name).HasColumnName("name").HasMaxLength(64).IsRequired();
        builder.HasIndex(l => l.Name).IsUnique().HasDatabaseName("UX_label_name");
    }
}
