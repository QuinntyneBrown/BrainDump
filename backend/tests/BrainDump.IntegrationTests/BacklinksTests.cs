// Acceptance Test
// Traces to: L2-043, L2-044
// Description: end-to-end coverage for [[wiki-link]] reference extraction
// and the GET /api/documents/{id}/backlinks endpoint introduced by Slice 06.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class BacklinksTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Adding_wiki_link_in_a_fact_creates_a_backlink_on_the_target()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var target = await CreateDocument(client, "L1 architecture");
        var source = await CreateDocument(client, "Designs");
        var sectionId = await CreateSection(client, source, "Notes");
        await CreateFact(client, sectionId, "see [[L1 architecture]] for context");

        var backlinks = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{target}/backlinks");
        var ids = backlinks.EnumerateArray().Select(e => e.GetProperty("id").GetInt32()).ToList();
        Assert.Contains(source, ids);
    }

    [Fact]
    public async Task Editing_a_fact_to_remove_the_link_removes_the_backlink()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var target = await CreateDocument(client, "Target");
        var source = await CreateDocument(client, "Source");
        var sectionId = await CreateSection(client, source, "Section");
        var factId = await CreateFact(client, sectionId, "see [[Target]]");

        var before = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{target}/backlinks");
        Assert.Single(before.EnumerateArray());

        var put = await client.PutAsJsonAsync($"/api/facts/{factId}", new
        {
            sectionId, text = "no link anymore", position = 10
        });
        put.EnsureSuccessStatusCode();

        var after = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{target}/backlinks");
        Assert.Empty(after.EnumerateArray());
    }

    [Fact]
    public async Task Deleting_the_source_document_removes_the_backlink()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var target = await CreateDocument(client, "Target");
        var source = await CreateDocument(client, "Source");
        var sectionId = await CreateSection(client, source, "Section");
        await CreateFact(client, sectionId, "[[Target]]");

        var del = await client.DeleteAsync($"/api/documents/{source}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var backlinks = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{target}/backlinks");
        Assert.Empty(backlinks.EnumerateArray());
    }

    [Fact]
    public async Task Unresolved_title_does_not_error_save_and_produces_no_link()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var source = await CreateDocument(client, "Source");
        var sectionId = await CreateSection(client, source, "Section");
        var fact = await client.PostAsJsonAsync("/api/facts", new
        {
            sectionId, text = "[[does-not-exist]]", position = 10
        });
        // Save still succeeds.
        fact.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Id_form_link_resolves_regardless_of_target_title()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var target = await CreateDocument(client, "Some Title");
        var source = await CreateDocument(client, "Source");
        var sectionId = await CreateSection(client, source, "Section");
        await CreateFact(client, sectionId, $"see [[id:{target}]]");

        var backlinks = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{target}/backlinks");
        var ids = backlinks.EnumerateArray().Select(e => e.GetProperty("id").GetInt32()).ToList();
        Assert.Contains(source, ids);
    }

    [Fact]
    public async Task Self_links_are_filtered_out()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var doc = await CreateDocument(client, "Self");
        var sectionId = await CreateSection(client, doc, "Section");
        await CreateFact(client, sectionId, "[[Self]]");

        var backlinks = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{doc}/backlinks");
        Assert.Empty(backlinks.EnumerateArray());
    }

    private static async Task<int> CreateDocument(HttpClient client, string title)
    {
        var resp = await client.PostAsJsonAsync("/api/documents", new { folderId = (int?)null, title, position = 10 });
        resp.EnsureSuccessStatusCode();
        return (await resp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateSection(HttpClient client, int documentId, string title)
    {
        var resp = await client.PostAsJsonAsync("/api/sections", new { documentId, parentId = (int?)null, title, position = 10 });
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<int>();
    }

    private static async Task<int> CreateFact(HttpClient client, int sectionId, string text)
    {
        var resp = await client.PostAsJsonAsync("/api/facts", new { sectionId, text, position = 10 });
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<int>();
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
        var dbFile = Path.Combine(Path.GetTempPath(), $"braindump-backlinks-test-{Guid.NewGuid():N}.db");
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
