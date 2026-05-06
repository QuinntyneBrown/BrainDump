using System.Security.Claims;
using BrainDump.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BrainDump.Api.Auth;

public static class UserBootstrapHandler
{
    public static async Task OnTokenValidated(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var email = principal?.FindFirst(ClaimTypes.Email)?.Value
                    ?? principal?.FindFirst("email")?.Value
                    ?? principal?.FindFirst("preferred_username")?.Value;

        if (string.IsNullOrWhiteSpace(email))
        {
            context.Fail("Token is missing an email claim; cannot bootstrap user.");
            return;
        }

        var sp = context.HttpContext.RequestServices;
        var users = sp.GetRequiredService<IUserRepository>();
        var current = sp.GetRequiredService<ICurrentUser>();

        var user = await FindOrCreateAsync(users, email, context.HttpContext.RequestAborted);

        current.UserId = user.Id;
        current.Email = user.Email;
    }

    private static async Task<Domain.Entities.User> FindOrCreateAsync(
        IUserRepository users, string email, CancellationToken ct)
    {
        var user = await users.FindByEmailAsync(email, ct);
        if (user is not null) return user;

        return await users.CreateAsync(email, passwordHash: null, ct);
    }
}
