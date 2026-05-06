using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Infrastructure.Persistence;

public class FolderRepository : IFolderRepository
{
    private readonly AppDbContext _db;

    public FolderRepository(AppDbContext db) => _db = db;

    public Task<Folder?> FindAsync(int id, CancellationToken ct = default) =>
        _db.Folders.FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<Folder> CreateAsync(int? parentId, string title, int position, CancellationToken ct = default)
    {
        var folder = new Folder { ParentId = parentId, Title = title, Position = position };
        _db.Folders.Add(folder);
        await _db.SaveChangesAsync(ct);
        return folder;
    }

    public async Task UpdateAsync(Folder folder, CancellationToken ct = default)
    {
        _db.Folders.Update(folder);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        // Walk descendant folder ids breadth-first; delete child documents
        // and folders in bottom-up order so the EF graph stays referentially
        // consistent.
        var ids = new HashSet<int> { id };
        var frontier = new Queue<int>();
        frontier.Enqueue(id);
        while (frontier.Count > 0)
        {
            var parentId = frontier.Dequeue();
            var children = await _db.Folders
                .Where(f => f.ParentId == parentId)
                .Select(f => f.Id)
                .ToListAsync(ct);
            foreach (var childId in children)
            {
                if (ids.Add(childId)) frontier.Enqueue(childId);
            }
        }

        var documents = await _db.Documents
            .Where(d => d.FolderId != null && ids.Contains(d.FolderId.Value))
            .ToListAsync(ct);
        _db.Documents.RemoveRange(documents);

        var folders = await _db.Folders
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);
        _db.Folders.RemoveRange(folders);

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Folder>> ListAllAsync(CancellationToken ct = default) =>
        await _db.Folders.OrderBy(f => f.Position).ToListAsync(ct);

    public async Task<IReadOnlyList<int>> GetDescendantIdsAsync(int folderId, CancellationToken ct = default)
    {
        var result = new List<int>();
        var frontier = new Queue<int>();
        frontier.Enqueue(folderId);
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var children = await _db.Folders
                .Where(f => f.ParentId == current)
                .Select(f => f.Id)
                .ToListAsync(ct);
            foreach (var c in children)
            {
                result.Add(c);
                frontier.Enqueue(c);
            }
        }
        return result;
    }
}
