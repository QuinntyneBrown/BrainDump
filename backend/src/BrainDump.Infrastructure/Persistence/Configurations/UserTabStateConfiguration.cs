using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrainDump.Infrastructure.Persistence.Configurations;

public class UserTabStateConfiguration : IEntityTypeConfiguration<UserTabState>
{
    public void Configure(EntityTypeBuilder<UserTabState> builder)
    {
        builder.ToTable("user_tab_state");
        builder.HasKey(t => t.UserId);
        builder.Property(t => t.UserId).HasColumnName("user_id");
        builder.Property(t => t.PanesJson).HasColumnName("panes_json").IsRequired();

        builder.HasOne(t => t.User)
            .WithOne()
            .HasForeignKey<UserTabState>(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
