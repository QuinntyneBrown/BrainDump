using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Documents;

public record DeleteDocument(int Id) : IRequest<Unit>;

public class DeleteDocumentHandler : IRequestHandler<DeleteDocument, Unit>
{
    private readonly IDocumentRepository _documents;

    public DeleteDocumentHandler(IDocumentRepository documents) => _documents = documents;

    public async Task<Unit> Handle(DeleteDocument request, CancellationToken cancellationToken)
    {
        if (await _documents.FindAsync(request.Id, cancellationToken) is null)
            throw new NotFoundException($"Document {request.Id} not found");

        await _documents.DeleteAsync(request.Id, cancellationToken);
        return Unit.Value;
    }
}
