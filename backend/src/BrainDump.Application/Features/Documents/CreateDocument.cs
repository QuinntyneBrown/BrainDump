using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Documents;

public record CreateDocument(int? FolderId, string Title, int Position) : IRequest<int>;

public class CreateDocumentHandler : IRequestHandler<CreateDocument, int>
{
    private readonly IDocumentRepository _documents;
    private readonly IFolderRepository _folders;

    public CreateDocumentHandler(IDocumentRepository documents, IFolderRepository folders)
    {
        _documents = documents;
        _folders = folders;
    }

    public async Task<int> Handle(CreateDocument request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title must not be empty");
        if (request.FolderId is int fid && await _folders.FindAsync(fid, cancellationToken) is null)
            throw new NotFoundException($"Folder {fid} not found");

        var doc = await _documents.CreateAsync(request.FolderId, request.Title.Trim(), request.Position, cancellationToken);
        return doc.Id;
    }
}
