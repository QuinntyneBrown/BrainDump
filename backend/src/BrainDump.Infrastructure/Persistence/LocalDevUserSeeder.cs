using BrainDump.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BrainDump.Infrastructure.Persistence;

public static class LocalDevUserSeeder
{
    public static async Task SeedAsync(
        AppDbContext db,
        IPasswordHasher hasher,
        IConfiguration config,
        CancellationToken ct = default)
    {
        if (!string.Equals(config["Jwt:UseLocalAuth"], "true", StringComparison.OrdinalIgnoreCase)) return;

        var email = config["Jwt:LocalAuth:DevEmail"];
        var password = config["Jwt:LocalAuth:DevPassword"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) return;

        var existing = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (existing is not null) return;

        db.Users.Add(new Domain.Entities.User
        {
            Email = email,
            PasswordHash = hasher.Hash(password),
            CreatedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync(ct);
    }
}
