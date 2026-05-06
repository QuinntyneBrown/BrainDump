// Acceptance Test
// Traces to: L2-032
// Description: Local sign-in (Jwt:UseLocalAuth=true) must cause the host to fail
// fast when the environment is not Development. This protects the production
// JWT validation path from being silently weakened by a misconfigured deploy.
//
// The guard runs at the top of Program.cs — before builder.Build() — so it sees
// configuration as constructed by WebApplication.CreateBuilder(args). That means
// WithWebHostBuilder/ConfigureAppConfiguration callbacks (which fire later, in
// Build()) don't influence it. We drive the test by setting environment variables
// the host reads at startup, scoped to the test via try/finally.

using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class LocalAuthSafetyTests
{
    [Fact]
    public void Host_fails_to_start_when_local_auth_enabled_outside_development()
    {
        using var _ = WithEnv(new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Production",
            ["Jwt__UseLocalAuth"] = "true",
            ["ConnectionStrings__DefaultConnection"] = "Data Source=:memory:",
            ["Database__Provider"] = "Sqlite",
            ["Database__EnsureCreatedOnStartup"] = "false",
        });

        var factory = new WebApplicationFactory<Program>();

        var ex = Assert.ThrowsAny<InvalidOperationException>(() =>
        {
            using var client = factory.CreateClient();
        });
        Assert.Contains("Jwt:UseLocalAuth", ex.Message);
        Assert.Contains("Development", ex.Message);

        factory.Dispose();
    }

    [Fact]
    public void Host_starts_when_local_auth_enabled_in_development()
    {
        using var _ = WithEnv(new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["Jwt__UseLocalAuth"] = "true",
            ["Jwt__LocalAuth__SigningKey"] = "test-signing-key-must-be-at-least-32-chars!!",
            ["Jwt__LocalAuth__Issuer"] = "test-issuer",
            ["Jwt__LocalAuth__Audience"] = "test-audience",
            ["ConnectionStrings__DefaultConnection"] = "Data Source=:memory:",
            ["Database__Provider"] = "Sqlite",
            ["Database__EnsureCreatedOnStartup"] = "false",
        });

        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        Assert.NotNull(client);
    }

    private static EnvScope WithEnv(Dictionary<string, string?> values) => new(values);

    private sealed class EnvScope : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = new();

        public EnvScope(Dictionary<string, string?> values)
        {
            foreach (var (key, value) in values)
            {
                _previous[key] = Environment.GetEnvironmentVariable(key);
                Environment.SetEnvironmentVariable(key, value);
            }
        }

        public void Dispose()
        {
            foreach (var (key, value) in _previous)
                Environment.SetEnvironmentVariable(key, value);
        }
    }
}
