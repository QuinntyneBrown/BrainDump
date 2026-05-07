using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Infrastructure.Persistence;

public class DocumentRepository : IDocumentRepository
{
    private readonly AppDbContext _db;

    public DocumentRepository(AppDbContext db) => _db = db;

    public Task<Document?> FindAsync(int id, CancellationToken ct = default) =>
        _db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<Document> CreateAsync(int? folderId, string title, int position, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var doc = new Document
        {
            FolderId = folderId,
            Title = title,
            Position = position,
            CreatedAt = now,
            UpdatedAt = now,
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync(ct);
        return doc;
    }

    public async Task UpdateAsync(Document document, CancellationToken ct = default)
    {
        document.UpdatedAt = DateTime.UtcNow;
        _db.Documents.Update(document);
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var doc = await _db.Documents
            .Include(d => d.Sections)
                .ThenInclude(s => s.Facts)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
        if (doc is null) return;

        // Remove inbound document_link rows (where this doc is the target)
        // explicitly. The source side cascades via the DB FK; the target
        // side uses ClientCascade per the SQL Server multi-cascade-path
        // restriction, so we clean it up here.
        var inbound = await _db.Set<DocumentLink>()
            .Where(dl => dl.TargetDocumentId == id)
            .ToListAsync(ct);
        if (inbound.Count > 0) _db.Set<DocumentLink>().RemoveRange(inbound);

        _db.Documents.Remove(doc);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Document>> ListAllAsync(CancellationToken ct = default) =>
        await _db.Documents.OrderBy(d => d.Position).ToListAsync(ct);
}
