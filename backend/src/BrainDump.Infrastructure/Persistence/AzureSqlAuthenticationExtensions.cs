using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Infrastructure.Persistence;

/// <summary>
/// Configures Azure SQL access tokens via DefaultAzureCredential (Managed Identity friendly).
/// No passwords or shared secrets are placed in the connection string.
/// </summary>
public static class AzureSqlAuthenticationExtensions
{
    private static readonly string[] AzureSqlScopes = new[] { "https://database.windows.net/.default" };

    public static DbContextOptionsBuilder UseAzureSqlAuthentication(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        TokenCredential? credential = null)
    {
        var tokenCredential = credential ?? new DefaultAzureCredential();

        var connection = new SqlConnection(connectionString);
        connection.AccessTokenCallback = async (ctx, ct) =>
        {
            var token = await tokenCredential.GetTokenAsync(
                new TokenRequestContext(AzureSqlScopes), ct);
            return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
        };

        return optionsBuilder.UseSqlServer(connection);
    }

    public static DbContextOptionsBuilder<TContext> UseAzureSqlAuthentication<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string connectionString,
        TokenCredential? credential = null) where TContext : DbContext
    {
        AzureSqlAuthenticationExtensions.UseAzureSqlAuthentication((DbContextOptionsBuilder)optionsBuilder, connectionString, credential);
        return optionsBuilder;
    }
}
