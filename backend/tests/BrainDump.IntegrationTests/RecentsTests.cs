// Acceptance Test
// Traces to: L2-047 (Recently Viewed)
// Description: end-to-end coverage for /api/documents/{id}/view and
// /api/recent introduced by Slice 04.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class RecentsTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Recording_a_view_makes_the_document_appear_in_recent()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docId = await CreateDocument(client, "alpha");
        var view = await client.PostAsync($"/api/documents/{docId}/view", null);
        Assert.Equal(HttpStatusCode.NoContent, view.StatusCode);

        var recent = await client.GetFromJsonAsync<JsonElement>("/api/recent");
        var entries = recent.EnumerateArray().ToList();
        Assert.Single(entries);
        Assert.Equal(docId, entries[0].GetProperty("id").GetInt32());
        Assert.Equal("alpha", entries[0].GetProperty("title").GetString());
    }

    [Fact]
    public async Task Re_viewing_a_document_moves_it_to_the_top_of_recent()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        var docB = await CreateDocument(client, "B");

        await client.PostAsync($"/api/documents/{docA}/view", null);
        await Task.Delay(15);
        await client.PostAsync($"/api/documents/{docB}/view", null);
        await Task.Delay(15);
        await client.PostAsync($"/api/documents/{docA}/view", null);

        var recent = await client.GetFromJsonAsync<JsonElement>("/api/recent");
        var ids = recent.EnumerateArray()
            .Select(e => e.GetProperty("id").GetInt32())
            .ToList();
        Assert.Equal(new[] { docA, docB }, ids);
    }

    [Fact]
    public async Task Deleted_documents_are_not_returned_by_recent()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docId = await CreateDocument(client, "ephemeral");
        await client.PostAsync($"/api/documents/{docId}/view", null);
        var del = await client.DeleteAsync($"/api/documents/{docId}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var recent = await client.GetFromJsonAsync<JsonElement>("/api/recent");
        Assert.Empty(recent.EnumerateArray());
    }

    [Fact]
    public async Task Recording_a_view_for_unknown_document_returns_404()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var resp = await client.PostAsync("/api/documents/9999/view", null);
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
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
        var dbFile = Path.Combine(Path.GetTempPath(), $"braindump-recents-test-{Guid.NewGuid():N}.db");
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
