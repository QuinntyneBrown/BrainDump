using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Folder> Folders { get; }
    DbSet<Document> Documents { get; }
    DbSet<Section> Sections { get; }
    DbSet<Fact> Facts { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
