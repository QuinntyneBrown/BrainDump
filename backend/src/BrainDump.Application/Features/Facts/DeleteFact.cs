using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Backlinks;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Facts;

public record DeleteFact(int Id) : IRequest<Unit>;

public class DeleteFactHandler : IRequestHandler<DeleteFact, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IDocumentLinkRefresher _links;

    public DeleteFactHandler(IAppDbContext db, IDocumentLinkRefresher links)
    {
        _db = db;
        _links = links;
    }

    public async Task<Unit> Handle(DeleteFact request, CancellationToken cancellationToken)
    {
        var fact = await _db.Facts.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Fact {request.Id} not found");

        var documentId = await _db.Sections
            .Where(s => s.Id == fact.SectionId)
            .Select(s => (int?)s.DocumentId)
            .FirstOrDefaultAsync(cancellationToken);

        _db.Facts.Remove(fact);
        await _db.SaveChangesAsync(cancellationToken);

        if (documentId is int docId)
            await _links.RefreshAsync(docId, cancellationToken);

        return Unit.Value;
    }
}
