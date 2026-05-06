using BrainDump.Domain.Entities;

namespace BrainDump.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<User> CreateAsync(string email, string? passwordHash, CancellationToken ct = default);
}
