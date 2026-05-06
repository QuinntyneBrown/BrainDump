using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;

namespace BrainDump.Application.Features.Documents;

public record UpdateDocument(int Id, string Title, int Position) : IRequest<Unit>;

public class UpdateDocumentHandler : IRequestHandler<UpdateDocument, Unit>
{
    private readonly IDocumentRepository _documents;

    public UpdateDocumentHandler(IDocumentRepository documents) => _documents = documents;

    public async Task<Unit> Handle(UpdateDocument request, CancellationToken cancellationToken)
    {
        var doc = await _documents.FindAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException($"Document {request.Id} not found");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Title must not be empty");

        doc.Title = request.Title.Trim();
        doc.Position = request.Position;
        await _documents.UpdateAsync(doc, cancellationToken);
        return Unit.Value;
    }
}
