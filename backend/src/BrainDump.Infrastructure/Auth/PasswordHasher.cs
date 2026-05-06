using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using IdentityHasher = Microsoft.AspNetCore.Identity.PasswordHasher<BrainDump.Domain.Entities.User>;
using PasswordVerificationResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace BrainDump.Infrastructure.Auth;

public class PasswordHasher : IPasswordHasher
{
    private static readonly User Sentinel = new();
    private readonly IdentityHasher _inner = new();

    public string Hash(string password) => _inner.HashPassword(Sentinel, password);

    public bool Verify(string hash, string password) =>
        _inner.VerifyHashedPassword(Sentinel, hash, password) != PasswordVerificationResult.Failed;
}
