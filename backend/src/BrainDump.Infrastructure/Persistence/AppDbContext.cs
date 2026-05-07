using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Fact> Facts => Set<Fact>();
    public DbSet<Label> Labels => Set<Label>();
    public DbSet<DocumentLabel> DocumentLabels => Set<DocumentLabel>();
    public DbSet<DocumentLink> DocumentLinks => Set<DocumentLink>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTabState> UserTabStates => Set<UserTabState>();
    public DbSet<UserDocumentView> UserDocumentViews => Set<UserDocumentView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
