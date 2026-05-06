using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BrainDump.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core tools at design time (Add-Migration, etc.).
/// Reads connection string from environment variable BRAINDUMP_CONNECTION,
/// or falls back to a localdb default.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("BRAINDUMP_CONNECTION")
            ?? "Server=(localdb)\\mssqllocaldb;Database=BrainDump;Trusted_Connection=True;";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }
}
