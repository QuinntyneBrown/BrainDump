using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("folder");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(f => f.ParentId).HasColumnName("parent_id");
        builder.Property(f => f.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(f => f.Position).HasColumnName("position").IsRequired();

        // SQL Server forbids ON DELETE CASCADE on a self-reference; the
        // application layer (DeleteFolder handler) walks the descendant tree
        // before removing rows. See task 02 design §3 and the same pattern on
        // Section in commit e88f70e.
        builder.HasOne(f => f.Parent)
            .WithMany(f => f.Children)
            .HasForeignKey(f => f.ParentId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasIndex(f => new { f.ParentId, f.Position })
            .HasDatabaseName("IX_folder_parent_id_position");
    }
}
