// Acceptance Test
// Traces to: L2-031 (#1, #2, #3, #4, #5)
// Description: Exercises the two-step PKCE local sign-in flow exposed by
// AuthController in Development. Drives WebApplicationFactory<Program> with
// Sqlite in-memory + Jwt:UseLocalAuth env vars so the host boots without a
// real database and the AuthController's "use local auth" gate is satisfied.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class LocalSignInTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Happy_path_returns_jwt_that_authorizes_protected_endpoint()
    {
        // L2-031 #1, #4
        using var _ = LocalAuthEnabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var (verifier, challenge) = NewPkcePair();

        var authorize = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = DevPassword,
            codeChallenge = challenge,
            codeChallengeMethod = "S256",
        });
        Assert.Equal(HttpStatusCode.OK, authorize.StatusCode);
        var authBody = await authorize.Content.ReadFromJsonAsync<JsonElement>();
        var code = authBody.GetProperty("code").GetString();
        Assert.False(string.IsNullOrWhiteSpace(code));

        var token = await client.PostAsJsonAsync("/api/auth/token", new
        {
            code,
            codeVerifier = verifier,
            grantType = "authorization_code",
        });
        Assert.Equal(HttpStatusCode.OK, token.StatusCode);
        var tokenBody = await token.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = tokenBody.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.Equal("Bearer", tokenBody.GetProperty("tokenType").GetString());
        Assert.True(tokenBody.GetProperty("expiresIn").GetInt32() > 0);

        // L2-031 #4: the token must satisfy the bearer pipeline. /api/tree
        // hits the DB on Sqlite in-memory which is empty — so we expect a
        // status that is *not* 401 (authorized to proceed), regardless of
        // whether the handler returns 200 or 500.
        using var authedReq = new HttpRequestMessage(HttpMethod.Get, "/api/tree");
        authedReq.Headers.Authorization = new("Bearer", accessToken);
        var authed = await client.SendAsync(authedReq);
        Assert.NotEqual(HttpStatusCode.Unauthorized, authed.StatusCode);

        // And confirm the same endpoint refuses an unauthenticated call.
        var anon = await client.GetAsync("/api/tree");
        Assert.Equal(HttpStatusCode.Unauthorized, anon.StatusCode);
    }

    [Fact]
    public async Task Wrong_credentials_return_401()
    {
        // L2-031 #2
        using var _ = LocalAuthEnabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var (_, challenge) = NewPkcePair();
        var resp = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = "wrong",
            codeChallenge = challenge,
            codeChallengeMethod = "S256",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Unsupported_code_challenge_method_returns_400()
    {
        // L2-031 #3 — unsupported codeChallengeMethod is malformed input.
        using var _ = LocalAuthEnabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var (_, challenge) = NewPkcePair();
        var resp = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = DevPassword,
            codeChallenge = challenge,
            codeChallengeMethod = "plain",
        });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Token_endpoint_with_unsupported_grant_type_returns_400()
    {
        // L2-031 #3
        using var _ = LocalAuthEnabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("/api/auth/token", new
        {
            code = "anything",
            codeVerifier = "anything",
            grantType = "client_credentials",
        });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Token_endpoint_with_mismatched_verifier_returns_401()
    {
        // L2-031 #3 — verifier that doesn't hash to the original challenge must
        // not produce a token. The endpoint returns 401 for this case (which
        // L2-031 #3 lists as acceptable alongside 400).
        using var _ = LocalAuthEnabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var (_, challenge) = NewPkcePair();
        var authorize = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = DevPassword,
            codeChallenge = challenge,
            codeChallengeMethod = "S256",
        });
        Assert.Equal(HttpStatusCode.OK, authorize.StatusCode);
        var code = (await authorize.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();

        var resp = await client.PostAsJsonAsync("/api/auth/token", new
        {
            code,
            codeVerifier = "completely-different-verifier",
            grantType = "authorization_code",
        });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Auth_endpoints_return_404_when_local_auth_disabled()
    {
        // L2-031 #5
        using var _ = LocalAuthDisabledEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var (_, challenge) = NewPkcePair();
        var authorize = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = DevPassword,
            codeChallenge = challenge,
            codeChallengeMethod = "S256",
        });
        Assert.Equal(HttpStatusCode.NotFound, authorize.StatusCode);

        var token = await client.PostAsJsonAsync("/api/auth/token", new
        {
            code = "x",
            codeVerifier = "x",
            grantType = "authorization_code",
        });
        Assert.Equal(HttpStatusCode.NotFound, token.StatusCode);
    }

    private static EnvScope LocalAuthEnabledEnv() => new(new()
    {
        ["ASPNETCORE_ENVIRONMENT"] = "Development",
        ["Jwt__UseLocalAuth"] = "true",
        ["Jwt__LocalAuth__SigningKey"] = "test-signing-key-must-be-at-least-32-chars!!",
        ["Jwt__LocalAuth__Issuer"] = "test-issuer",
        ["Jwt__LocalAuth__Audience"] = "test-audience",
        ["Jwt__LocalAuth__DevEmail"] = DevEmail,
        ["Jwt__LocalAuth__DevPassword"] = DevPassword,
        ["ConnectionStrings__DefaultConnection"] = "Data Source=:memory:",
        ["Database__Provider"] = "Sqlite",
        ["Database__EnsureCreatedOnStartup"] = "false",
    });

    private static EnvScope LocalAuthDisabledEnv() => new(new()
    {
        ["ASPNETCORE_ENVIRONMENT"] = "Development",
        ["Jwt__UseLocalAuth"] = "false",
        ["Jwt__Authority"] = "https://login.example/v2.0",
        ["Jwt__Audience"] = "test",
        ["ConnectionStrings__DefaultConnection"] = "Data Source=:memory:",
        ["Database__Provider"] = "Sqlite",
        ["Database__EnsureCreatedOnStartup"] = "false",
    });

    private static (string Verifier, string Challenge) NewPkcePair()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var verifier = Base64Url(bytes);
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        return (verifier, challenge);
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

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
