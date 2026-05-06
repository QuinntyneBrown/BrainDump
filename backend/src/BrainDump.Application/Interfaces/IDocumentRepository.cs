using BrainDump.Domain.Entities;

namespace BrainDump.Application.Interfaces;

public interface IDocumentRepository
{
    Task<Document?> FindAsync(int id, CancellationToken ct = default);
    Task<Document> CreateAsync(int? folderId, string title, int position, CancellationToken ct = default);
    Task UpdateAsync(Document document, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> ListAllAsync(CancellationToken ct = default);
}
