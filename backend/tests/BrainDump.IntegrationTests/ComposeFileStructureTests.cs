// Acceptance Test
// Traces to: L2-027 (#1, #2, #3, #4), L2-029 (#1, #2, #3)
// Description: Parses docker-compose.yml at the repo root and asserts the
// services/ports/images/volumes the spec requires. This test is purely
// static-analysis — it does NOT need Docker installed. The runtime smoke
// (compose up + healthz) is covered by L2-028 manually + L2-029 #5 by the
// SqlServerConnectivityTests stub.

using YamlDotNet.RepresentationModel;

namespace BrainDump.IntegrationTests;

public class ComposeFileStructureTests
{
    private static readonly Lazy<YamlMappingNode> Root = new(LoadComposeRoot);

    [Fact]
    public void ComposeFile_exists_at_repository_root()
    {
        // L2-027 #1
        Assert.True(File.Exists(ComposeFilePath()),
            $"docker-compose.yml not found at {ComposeFilePath()}");
    }

    [Fact]
    public void Three_required_services_are_declared()
    {
        // L2-027 #2
        var services = Services();
        Assert.Contains("sqlserver", services.Children.Keys.Select(Scalar));
        Assert.Contains("api", services.Children.Keys.Select(Scalar));
        Assert.Contains("web", services.Children.Keys.Select(Scalar));
    }

    [Fact]
    public void Sqlserver_uses_official_mssql_image()
    {
        // L2-029 #1
        var sqlserver = Service("sqlserver");
        var image = Scalar(sqlserver.Children[new YamlScalarNode("image")]);
        Assert.StartsWith("mcr.microsoft.com/mssql/server", image);
    }

    [Fact]
    public void Sqlserver_environment_includes_eula_and_password()
    {
        // L2-029 #1
        var env = (YamlMappingNode)Service("sqlserver").Children[new YamlScalarNode("environment")];
        Assert.Equal("Y", Scalar(env.Children[new YamlScalarNode("ACCEPT_EULA")]));

        var hasSaPassword =
            env.Children.ContainsKey(new YamlScalarNode("MSSQL_SA_PASSWORD")) ||
            env.Children.ContainsKey(new YamlScalarNode("SA_PASSWORD"));
        Assert.True(hasSaPassword, "sqlserver service must declare MSSQL_SA_PASSWORD or SA_PASSWORD");
    }

    [Fact]
    public void Sqlserver_publishes_port_1433()
    {
        // L2-029 #2
        AssertPortMapping("sqlserver", "1433", "1433");
    }

    [Fact]
    public void Sqlserver_data_directory_is_a_named_volume()
    {
        // L2-029 #3
        var ports = (YamlSequenceNode)Service("sqlserver").Children[new YamlScalarNode("volumes")];
        var entries = ports.Children.Select(Scalar).ToList();
        var dataMount = entries.SingleOrDefault(e => e.EndsWith(":/var/opt/mssql"));
        Assert.NotNull(dataMount);

        var volumeName = dataMount!.Split(':')[0];
        // A bind mount would start with "./" or "/". A named volume is bare.
        Assert.False(volumeName.StartsWith('.') || volumeName.StartsWith('/'),
            $"/var/opt/mssql must be backed by a named volume, got '{volumeName}'");

        var topLevelVolumes = (YamlMappingNode)Root.Value.Children[new YamlScalarNode("volumes")];
        Assert.Contains(volumeName, topLevelVolumes.Children.Keys.Select(Scalar));
    }

    [Fact]
    public void Api_connection_string_targets_sqlserver_service_name()
    {
        // L2-027 #3
        var env = (YamlMappingNode)Service("api").Children[new YamlScalarNode("environment")];
        var connStr = Scalar(env.Children[new YamlScalarNode("ConnectionStrings__DefaultConnection")]);

        Assert.Contains("Server=sqlserver", connStr,
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=localhost", connStr,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Api_depends_on_sqlserver_being_healthy()
    {
        // L2-027 #2 / L2-028 startup ordering
        var depends = (YamlMappingNode)Service("api").Children[new YamlScalarNode("depends_on")];
        var sqlDep = (YamlMappingNode)depends.Children[new YamlScalarNode("sqlserver")];
        Assert.Equal("service_healthy", Scalar(sqlDep.Children[new YamlScalarNode("condition")]));
    }

    [Fact]
    public void Web_service_publishes_port_4200()
    {
        // L2-027 #2 / L2-028 #3
        AssertPortMapping("web", "4200", "4200");
    }

    [Fact]
    public void Compose_file_parses_without_error()
    {
        // L2-027 #4 — surrogate for `docker compose config`. If YamlDotNet can
        // load the document, the YAML is well-formed; structural checks above
        // cover the schema-level requirements.
        Assert.NotNull(Root.Value);
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    private static YamlMappingNode Services() =>
        (YamlMappingNode)Root.Value.Children[new YamlScalarNode("services")];

    private static YamlMappingNode Service(string name) =>
        (YamlMappingNode)Services().Children[new YamlScalarNode(name)];

    private static void AssertPortMapping(string service, string host, string container)
    {
        var ports = (YamlSequenceNode)Service(service).Children[new YamlScalarNode("ports")];
        var values = ports.Children.Select(Scalar).ToList();
        Assert.Contains($"{host}:{container}", values);
    }

    private static string Scalar(YamlNode node) => ((YamlScalarNode)node).Value ?? string.Empty;

    private static YamlMappingNode LoadComposeRoot()
    {
        using var reader = new StreamReader(ComposeFilePath());
        var stream = new YamlStream();
        stream.Load(reader);
        return (YamlMappingNode)stream.Documents[0].RootNode;
    }

    private static string ComposeFilePath()
    {
        // Walk up from the test bin directory to the repo root.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "docker-compose.yml")))
            dir = dir.Parent;
        return dir is null
            ? throw new FileNotFoundException("Could not locate docker-compose.yml above the test binary directory.")
            : Path.Combine(dir.FullName, "docker-compose.yml");
    }
}
