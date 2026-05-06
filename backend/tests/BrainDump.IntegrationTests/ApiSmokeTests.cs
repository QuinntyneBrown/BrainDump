using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace BrainDump.IntegrationTests;

/// <summary>
/// Smoke test using WebApplicationFactory&lt;Program&gt; — verifies the host can boot
/// and that protected endpoints return 401 without credentials. Skips when no
/// database connection is configured (the host requires DefaultConnection at startup).
/// </summary>
public class ApiSmokeTests
{
    [Fact]
    public async Task Tree_endpoint_requires_authentication()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("BRAINDUMP_TEST_CONNECTION")))
        {
            // Skip — host configuration depends on a connection string.
            return;
        }

        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/tree");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
