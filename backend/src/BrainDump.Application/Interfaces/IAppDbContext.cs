using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Folder> Folders { get; }
    DbSet<Document> Documents { get; }
    DbSet<Section> Sections { get; }
    DbSet<Fact> Facts { get; }
    DbSet<Label> Labels { get; }
    DbSet<DocumentLabel> DocumentLabels { get; }
    DbSet<User> Users { get; }
    DbSet<UserTabState> UserTabStates { get; }
    DbSet<UserDocumentView> UserDocumentViews { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
