using BrainDump.Application.Interfaces;
using BrainDump.Infrastructure.Auth;
using BrainDump.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrainDump.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured");

        // Database:Provider selects the EF provider. Default is SqlServer for both
        // local dev (against the Compose-launched container) and production (Azure SQL).
        // The Sqlite branch remains as an opt-in fallback for file-based dev workflows.
        var provider = configuration["Database:Provider"] ?? "SqlServer";

        services.AddDbContext<AppDbContext>(options =>
        {
            switch (provider.ToLowerInvariant())
            {
                case "sqlite":
                    options.UseSqlite(connectionString);
                    break;
                case "sqlserver":
                    if (UsesAzureSqlPasswordlessAuth(connectionString))
                        options.UseAzureSqlAuthentication(connectionString);
                    else
                        options.UseSqlServer(connectionString);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported Database:Provider value '{provider}'. Use 'SqlServer' or 'Sqlite'.");
            }
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFolderRepository, FolderRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUserAccessor>();

        return services;
    }

    // Use the AccessTokenCallback path only when targeting Azure SQL *and* the
    // connection string doesn't already specify an Authentication mode — the
    // callback and Authentication=... are mutually exclusive in SqlClient.
    private static bool UsesAzureSqlPasswordlessAuth(string connectionString)
    {
        SqlConnectionStringBuilder builder;
        try { builder = new SqlConnectionStringBuilder(connectionString); }
        catch { return false; }

        var host = builder.DataSource ?? string.Empty;
        var isAzureSql = host.Contains(".database.windows.net", StringComparison.OrdinalIgnoreCase);
        var hasExplicitAuth = builder.Authentication != SqlAuthenticationMethod.NotSpecified;

        return isAzureSql && !hasExplicitAuth;
    }
}
