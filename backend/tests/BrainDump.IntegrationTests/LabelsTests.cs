// Acceptance Test
// Traces to: L2-041, L2-042
// Description: end-to-end coverage for /api/documents/{id}/labels and
// /api/labels introduced by Slice 05.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class LabelsTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Set_labels_makes_them_appear_on_workspace_document()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docId = await CreateDocument(client, "alpha");
        var put = await client.PutAsJsonAsync($"/api/documents/{docId}/labels", new
        {
            labels = new[] { "engineering", "wip" }
        });
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var ws = await client.GetFromJsonAsync<JsonElement>("/api/workspace");
        var doc = ws.GetProperty("documents").EnumerateArray().Single(d => d.GetProperty("id").GetInt32() == docId);
        var labels = doc.GetProperty("labels").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Equal(new[] { "engineering", "wip" }, labels);
    }

    [Fact]
    public async Task Workspace_labels_lists_every_applied_name_alphabetically()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        var docB = await CreateDocument(client, "B");
        await SetLabels(client, docA, new[] { "wip" });
        await SetLabels(client, docB, new[] { "engineering", "wip" });

        var labels = await client.GetFromJsonAsync<List<string>>("/api/labels");
        Assert.Equal(new[] { "engineering", "wip" }, labels);
    }

    [Fact]
    public async Task Removing_a_label_from_one_doc_keeps_it_in_workspace_when_used_elsewhere()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        var docB = await CreateDocument(client, "B");
        await SetLabels(client, docA, new[] { "wip" });
        await SetLabels(client, docB, new[] { "wip" });

        // Strip from A.
        await SetLabels(client, docA, Array.Empty<string>());

        var labels = await client.GetFromJsonAsync<List<string>>("/api/labels");
        Assert.Contains("wip", labels);
    }

    [Fact]
    public async Task Removing_a_label_from_the_last_doc_leaves_it_in_workspace_for_reuse()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, "A");
        await SetLabels(client, docA, new[] { "wip" });
        await SetLabels(client, docA, Array.Empty<string>());

        var labels = await client.GetFromJsonAsync<List<string>>("/api/labels");
        // The label row stays — design accepts vocabulary lingering.
        Assert.Contains("wip", labels);
    }

    [Fact]
    public async Task Setting_labels_on_unknown_document_returns_404()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var resp = await client.PutAsJsonAsync("/api/documents/9999/labels", new
        {
            labels = new[] { "x" }
        });
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Duplicate_input_labels_collapse_to_one_pair()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docId = await CreateDocument(client, "alpha");
        await SetLabels(client, docId, new[] { "WIP", "wip", " #wip " });

        var ws = await client.GetFromJsonAsync<JsonElement>("/api/workspace");
        var doc = ws.GetProperty("documents").EnumerateArray().Single(d => d.GetProperty("id").GetInt32() == docId);
        var labels = doc.GetProperty("labels").EnumerateArray().Select(e => e.GetString()).ToList();
        Assert.Single(labels);
    }

    private static async Task<int> CreateDocument(HttpClient client, string title)
    {
        var resp = await client.PostAsJsonAsync("/api/documents", new { folderId = (int?)null, title, position = 10 });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task SetLabels(HttpClient client, int docId, IReadOnlyList<string> labels)
    {
        var resp = await client.PutAsJsonAsync($"/api/documents/{docId}/labels", new { labels });
        resp.EnsureSuccessStatusCode();
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
        var dbFile = Path.Combine(Path.GetTempPath(), $"braindump-labels-test-{Guid.NewGuid():N}.db");
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
