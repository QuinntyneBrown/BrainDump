using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Move;

public enum MoveKind
{
    Folder,
    Document
}

public record MoveNode(MoveKind Kind, int Id, int? TargetParentId, int Position) : IRequest<Unit>;

public class MoveNodeHandler : IRequestHandler<MoveNode, Unit>
{
    private readonly IFolderRepository _folders;
    private readonly IDocumentRepository _documents;

    public MoveNodeHandler(IFolderRepository folders, IDocumentRepository documents)
    {
        _folders = folders;
        _documents = documents;
    }

    public async Task<Unit> Handle(MoveNode request, CancellationToken cancellationToken)
    {
        switch (request.Kind)
        {
            case MoveKind.Folder:
                await MoveFolder(request, cancellationToken);
                break;
            case MoveKind.Document:
                await MoveDocument(request, cancellationToken);
                break;
            default:
                throw new ValidationException($"Unsupported move kind '{request.Kind}'");
        }
        return Unit.Value;
    }

    private async Task MoveFolder(MoveNode request, CancellationToken ct)
    {
        var folder = await _folders.FindAsync(request.Id, ct)
            ?? throw new NotFoundException($"Folder {request.Id} not found");

        if (request.TargetParentId is int targetId)
        {
            if (targetId == request.Id)
                throw new ValidationException("Cannot move a folder into itself");
            var descendants = await _folders.GetDescendantIdsAsync(request.Id, ct);
            if (descendants.Contains(targetId))
                throw new ValidationException("Cannot move a folder into one of its descendants");
            if (await _folders.FindAsync(targetId, ct) is null)
                throw new NotFoundException($"Target folder {targetId} not found");
        }

        folder.ParentId = request.TargetParentId;
        folder.Position = request.Position;
        await _folders.UpdateAsync(folder, ct);
    }

    private async Task MoveDocument(MoveNode request, CancellationToken ct)
    {
        var doc = await _documents.FindAsync(request.Id, ct)
            ?? throw new NotFoundException($"Document {request.Id} not found");
        if (request.TargetParentId is int targetId
            && await _folders.FindAsync(targetId, ct) is null)
            throw new NotFoundException($"Target folder {targetId} not found");

        doc.FolderId = request.TargetParentId;
        doc.Position = request.Position;
        await _documents.UpdateAsync(doc, ct);
    }
}
