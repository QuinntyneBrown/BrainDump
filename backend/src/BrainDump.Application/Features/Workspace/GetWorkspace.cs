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

        var documents = await _db.Documents
            .AsNoTracking()
            .OrderBy(d => d.Position)
            .Select(d => new DocumentDto(d.Id, d.FolderId, d.Title, d.Position, d.CreatedAt, d.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new WorkspaceDto(folders, documents);
    }
}
