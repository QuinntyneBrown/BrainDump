using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Backlinks;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Facts;

public record CreateFact(int SectionId, string Text, int Position) : IRequest<int>;

public class CreateFactHandler : IRequestHandler<CreateFact, int>
{
    private readonly IAppDbContext _db;
    private readonly IDocumentLinkRefresher _links;

    public CreateFactHandler(IAppDbContext db, IDocumentLinkRefresher links)
    {
        _db = db;
        _links = links;
    }

    public async Task<int> Handle(CreateFact request, CancellationToken cancellationToken)
    {
        var documentId = await _db.Sections
            .Where(s => s.Id == request.SectionId)
            .Select(s => (int?)s.DocumentId)
            .FirstOrDefaultAsync(cancellationToken);
        if (documentId is null)
            throw new ValidationException($"Section {request.SectionId} does not exist");

        var fact = new Fact
        {
            SectionId = request.SectionId,
            Text = request.Text,
            Position = request.Position
        };

        _db.Facts.Add(fact);
        await _db.SaveChangesAsync(cancellationToken);
        await _links.RefreshAsync(documentId.Value, cancellationToken);
        return fact.Id;
    }
}
