// Acceptance Test
// Traces to: L2-033..L2-038
// Description: end-to-end coverage of the new folder + document + workspace
// + move + document-tree endpoints introduced by Slice 02. Drives the host
// with the same Sqlite-in-temp-file pattern as LocalSignInTests so SQL Server
// container availability is not required for the suite to run.

using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BrainDump.IntegrationTests;

public class WorkspaceTests
{
    private const string DevEmail = "user@braindump.dev";
    private const string DevPassword = "Password1!";

    [Fact]
    public async Task Create_folder_and_document_appears_in_workspace()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var folderId = await CreateFolder(client, parentId: null, title: "Engineering", position: 10);
        var docId = await CreateDocument(client, folderId: folderId, title: "brain-dump.md", position: 10);

        var ws = await client.GetFromJsonAsync<JsonElement>("/api/workspace");
        var folders = ws.GetProperty("folders").EnumerateArray().ToList();
        var documents = ws.GetProperty("documents").EnumerateArray().ToList();

        Assert.Contains(folders, f => f.GetProperty("id").GetInt32() == folderId
            && f.GetProperty("title").GetString() == "Engineering");
        Assert.Contains(documents, d => d.GetProperty("id").GetInt32() == docId
            && d.GetProperty("folderId").GetInt32() == folderId
            && d.GetProperty("title").GetString() == "brain-dump.md");
    }

    [Fact]
    public async Task Document_tree_returns_404_for_unknown_id()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var resp = await client.GetAsync("/api/documents/9999/tree");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }

    [Fact]
    public async Task Document_tree_lists_only_sections_belonging_to_the_document()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docA = await CreateDocument(client, null, "A", 10);
        var docB = await CreateDocument(client, null, "B", 20);
        var sectionInA = await CreateSection(client, docA, parentId: null, title: "Root A", position: 10);
        var sectionInB = await CreateSection(client, docB, parentId: null, title: "Root B", position: 10);

        var treeA = await client.GetFromJsonAsync<JsonElement>($"/api/documents/{docA}/tree");
        var sectionsA = treeA.GetProperty("sections").EnumerateArray().Select(s => s.GetProperty("id").GetInt32()).ToList();
        Assert.Contains(sectionInA, sectionsA);
        Assert.DoesNotContain(sectionInB, sectionsA);
    }

    [Fact]
    public async Task Move_folder_into_descendant_returns_400()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var root = await CreateFolder(client, null, "Root", 10);
        var child = await CreateFolder(client, root, "Child", 10);

        var resp = await client.PostAsJsonAsync("/api/move", new
        {
            kind = "Folder",
            id = root,
            targetParentId = (int?)child,
            position = 10,
        });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task Delete_document_cascades_to_sections_and_facts()
    {
        using var env = NewEnv();
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        var token = await SignIn(client);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);

        var docId = await CreateDocument(client, null, "Disposable", 10);
        var sectionId = await CreateSection(client, docId, null, "Root", 10);
        var factResp = await client.PostAsJsonAsync("/api/facts", new
        {
            sectionId, text = "fact 1", position = 10
        });
        factResp.EnsureSuccessStatusCode();

        var del = await client.DeleteAsync($"/api/documents/{docId}");
        Assert.Equal(HttpStatusCode.NoContent, del.StatusCode);

        var get = await client.GetAsync($"/api/documents/{docId}/tree");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    private static async Task<int> CreateFolder(HttpClient client, int? parentId, string title, int position)
    {
        var resp = await client.PostAsJsonAsync("/api/folders", new { parentId, title, position });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateDocument(HttpClient client, int? folderId, string title, int position)
    {
        var resp = await client.PostAsJsonAsync("/api/documents", new { folderId, title, position });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetInt32();
    }

    private static async Task<int> CreateSection(HttpClient client, int documentId, int? parentId, string title, int position)
    {
        var resp = await client.PostAsJsonAsync("/api/sections", new { documentId, parentId, title, position });
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
        var dbFile = Path.Combine(Path.GetTempPath(), $"braindump-ws-test-{Guid.NewGuid():N}.db");
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
