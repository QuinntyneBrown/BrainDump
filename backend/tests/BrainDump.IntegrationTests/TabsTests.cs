// Acceptance Test
// Traces to: L2-039 (Open Tabs Persistence)
// Description: end-to-end coverage for /api/tabs read/write per Slice 03.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class TabsTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Get_tabs_returns_default_empty_state_for_first_request()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var resp = await client.GetFromJsonAsync<JsonElement>("/api/tabs");
        var panes = resp.GetProperty("panes").EnumerateArray().ToList();
        Assert.Single(panes);
        Assert.Empty(panes[0].GetProperty("tabs").EnumerateArray());
        Assert.Equal(-1, panes[0].GetProperty("activeIndex").GetInt32());
    }

    [Fact]
    public async Task Put_then_get_round_trips_pane_state()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        var docB = await CreateDocument(client, "B");

        var put = await client.PutAsJsonAsync("/api/tabs", new
        {
            panes = new[]
            {
                new { tabs = new[] { docA, docB }, activeIndex = 1 },
            },
        });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var got = await client.GetFromJsonAsync<JsonElement>("/api/tabs");
        var panes = got.GetProperty("panes").EnumerateArray().ToList();
        Assert.Single(panes);
        var tabs = panes[0].GetProperty("tabs").EnumerateArray().Select(e => e.GetInt32()).ToList();
        Assert.Equal(new[] { docA, docB }, tabs);
        Assert.Equal(1, panes[0].GetProperty("activeIndex").GetInt32());
    }

    [Fact]
    public async Task Get_filters_out_deleted_documents_and_clamps_active_index()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        var docB = await CreateDocument(client, "B");

        var put = await client.PutAsJsonAsync("/api/tabs", new
        {
            panes = new[]
            {
                new { tabs = new[] { docA, docB }, activeIndex = 1 },
            },
        });
        put.EnsureSuccessStatusCode();

        var del = await client.DeleteAsync($"/api/documents/{docB}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var got = await client.GetFromJsonAsync<JsonElement>("/api/tabs");
        var panes = got.GetProperty("panes").EnumerateArray().ToList();
        Assert.Single(panes);
        var tabs = panes[0].GetProperty("tabs").EnumerateArray().Select(e => e.GetInt32()).ToList();
        Assert.Equal(new[] { docA }, tabs);
        // ActiveIndex 1 was clamped to 0 because docB was filtered out.
        Assert.Equal(0, panes[0].GetProperty("activeIndex").GetInt32());
    }

    [Fact]
    public async Task Put_with_too_many_panes_returns_400()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var resp = await client.PutAsJsonAsync("/api/tabs", new
        {
            panes = new[]
            {
                new { tabs = Array.Empty<int>(), activeIndex = -1 },
                new { tabs = Array.Empty<int>(), activeIndex = -1 },
                new { tabs = Array.Empty<int>(), activeIndex = -1 },
            },
        });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    private static async Task<int> CreateDocument(HttpClient client, string title)
    {
        var resp = await client.PostAsJsonAsync("/api/documents", new { folderId = (int?)null, title, position = 10 });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<string> SignIn(HttpClient client)
    {
        var (verifier, challenge) = NewPkcePair();
        var authorize = await client.PostAsJsonAsync("/api/auth/authorize", new
        {
            email = DevEmail,
            password = DevPassword,
            codeChallenge = challenge,
            codeChallengeMethod = "S256",
        });
        authorize.EnsureSuccessStatusCode();
        var code = (await authorize.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("code").GetString();
        var token = await client.PostAsJsonAsync("/api/auth/token", new
        {
            code,
            codeVerifier = verifier,
            grantType = "authorization_code",
        });
        token.EnsureSuccessStatusCode();
        return (await token.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("accessToken").GetString()!;
    }

    private static TestEnv NewEnv()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"braindump-tabs-test-{Guid.NewGuid():N}.db");
        return new TestEnv(dbFile, new()
        {
            ["ASPNETCORE_ENVIRONMENT"] = "Development",
            ["Jwt__UseLocalAuth"] = "true",
            ["Jwt__LocalAuth__SigningKey"] = "test-signing-key-must-be-at-least-32-chars!!",
            ["Jwt__LocalAuth__Issuer"] = "test-issuer",
            ["Jwt__LocalAuth__Audience"] = "test-audience",
            ["Jwt__LocalAuth__DevEmail"] = DevEmail,
            ["Jwt__LocalAuth__DevPassword"] = DevPassword,
            ["ConnectionStrings__DefaultConnection"] = $"Data Source={dbFile}",
            ["Database__Provider"] = "Sqlite",
            ["Database__EnsureCreatedOnStartup"] = "true",
        });
    }

    private static (string Verifier, string Challenge) NewPkcePair()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var verifier = Base64Url(bytes);
        var challenge = Base64Url(SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        return (verifier, challenge);
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class TestEnv : IDisposable
    {
        private readonly Dictionary<string, string?> _previous = new();
        private readonly string _dbFile;

        public TestEnv(string dbFile, Dictionary<string, string?> values)
        {
            _dbFile = dbFile;
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
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { if (File.Exists(_dbFile)) File.Delete(_dbFile); } catch { }
        }
    }
}
