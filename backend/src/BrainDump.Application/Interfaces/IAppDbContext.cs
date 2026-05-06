using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Section> Sections { get; }
    DbSet<Fact> Facts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
