using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Backlinks;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Facts;

public record UpdateFact(int Id, int SectionId, string Text, int Position) : IRequest<Unit>;

public class UpdateFactHandler : IRequestHandler<UpdateFact, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IDocumentLinkRefresher _links;

    public UpdateFactHandler(IAppDbContext db, IDocumentLinkRefresher links)
    {
        _db = db;
        _links = links;
    }

    public async Task<Unit> Handle(UpdateFact request, CancellationToken cancellationToken)
    {
        var fact = await _db.Facts.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Fact {request.Id} not found");

        fact.SectionId = request.SectionId;
        fact.Text = request.Text;
        fact.Position = request.Position;

        await _db.SaveChangesAsync(cancellationToken);

        var documentId = await _db.Sections
            .Where(s => s.Id == fact.SectionId)
            .Select(s => (int?)s.DocumentId)
            .FirstOrDefaultAsync(cancellationToken);
        if (documentId is int docId)
            await _links.RefreshAsync(docId, cancellationToken);

        return Unit.Value;
    }
}
