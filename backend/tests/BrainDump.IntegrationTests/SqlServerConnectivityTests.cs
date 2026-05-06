using BrainDump.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BrainDump.IntegrationTests;

/// <summary>
/// Integration test stub that demonstrates the test connects to a real SQL Server.
/// Skips when no connection string is provided (CI without a database).
/// Configure via the BRAINDUMP_TEST_CONNECTION environment variable.
/// </summary>
public class SqlServerConnectivityTests
{
    private const string ConnectionEnvVar = "BRAINDUMP_TEST_CONNECTION";

    [Fact]
    public async Task Can_open_connection_to_configured_sql_server()
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionEnvVar);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Skip when no real database is configured. The point of this stub
            // is to demonstrate the wiring without forcing every CI run to have SQL.
            return;
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var db = new AppDbContext(options);
        var canConnect = await db.Database.CanConnectAsync();

        Assert.True(canConnect, "Failed to open a connection to the configured SQL Server");
    }
}
