using BrainDump.Domain.Entities;

namespace BrainDump.Application.Interfaces;

public interface IFolderRepository
{
    Task<Folder?> FindAsync(int id, CancellationToken ct = default);
    Task<Folder> CreateAsync(int? parentId, string title, int position, CancellationToken ct = default);
    Task UpdateAsync(Folder folder, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Folder>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetDescendantIdsAsync(int folderId, CancellationToken ct = default);
}
