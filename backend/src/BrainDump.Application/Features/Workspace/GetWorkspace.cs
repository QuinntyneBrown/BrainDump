using BrainDump.Application.DTOs;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Workspace;

public record GetWorkspace() : IRequest<WorkspaceDto>;

public class GetWorkspaceHandler : IRequestHandler<GetWorkspace, WorkspaceDto>
{
    private readonly IAppDbContext _db;

    public GetWorkspaceHandler(IAppDbContext db) => _db = db;

    public async Task<WorkspaceDto> Handle(GetWorkspace request, CancellationToken cancellationToken)
    {
        var folders = await _db.Folders
            .AsNoTracking()
            .OrderBy(f => f.Position)
            .Select(f => new FolderDto(f.Id, f.ParentId, f.Title, f.Position))
            .ToListAsync(cancellationToken);

        // Group document_label rows by document so we can stitch the
        // labels into each DocumentDto in a single round-trip.
        var labelsByDoc = await _db.DocumentLabels
            .AsNoTracking()
            .Join(_db.Labels.AsNoTracking(),
                dl => dl.LabelId,
                l => l.Id,
                (dl, l) => new { dl.DocumentId, l.Name })
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var labelsLookup = labelsByDoc
            .GroupBy(x => x.DocumentId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(x => x.Name).ToList());

        var rawDocs = await _db.Documents
            .AsNoTracking()
            .OrderBy(d => d.Position)
            .Select(d => new { d.Id, d.FolderId, d.Title, d.Position, d.CreatedAt, d.UpdatedAt })
            .ToListAsync(cancellationToken);

        var documents = rawDocs
            .Select(d => new DocumentDto(
                d.Id, d.FolderId, d.Title, d.Position, d.CreatedAt, d.UpdatedAt,
                labelsLookup.TryGetValue(d.Id, out var ls) ? ls : Array.Empty<string>()))
            .ToList();

        return new WorkspaceDto(folders, documents);
    }
}
