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

        builder.HasOne(s => s.Parent)
            .WithMany(s => s.Children)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ParentId, s.Position })
            .HasDatabaseName("IX_section_parent_id_position");
    }
}
