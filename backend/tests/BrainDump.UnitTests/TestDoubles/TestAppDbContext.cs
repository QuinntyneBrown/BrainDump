using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.UnitTests.TestDoubles;

/// <summary>
/// In-memory EF Core context that satisfies IAppDbContext for handler tests.
/// </summary>
public class TestAppDbContext : DbContext, IAppDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options) : base(options) { }

    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<Fact> Facts => Set<Fact>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserTabState> UserTabStates => Set<UserTabState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // UserTabState's primary key is UserId, not Id, so EF Core's
        // convention-based key discovery doesn't find one. Configure it
        // explicitly here since this test double lives outside the
        // Infrastructure assembly that owns the real configuration.
        modelBuilder.Entity<UserTabState>().HasKey(t => t.UserId);
    }

    public static TestAppDbContext CreateInMemory()
    {
        var options = new DbContextOptionsBuilder<TestAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestAppDbContext(options);
    }
}
