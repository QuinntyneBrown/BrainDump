using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class FactConfiguration : IEntityTypeConfiguration<Fact>
{
    public void Configure(EntityTypeBuilder<Fact> builder)
    {
        builder.ToTable("fact", t => t.HasCheckConstraint("CK_fact_text_nonempty", "text <> ''"));
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").UseIdentityColumn();
        builder.Property(f => f.SectionId).HasColumnName("section_id").IsRequired();
        builder.Property(f => f.Text).HasColumnName("text").IsRequired();
        builder.Property(f => f.Position).HasColumnName("position").IsRequired();

        builder.HasOne(f => f.Section)
            .WithMany(s => s.Facts)
            .HasForeignKey(f => f.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.SectionId, f.Position })
            .HasDatabaseName("IX_fact_section_id_position");
    }
}
