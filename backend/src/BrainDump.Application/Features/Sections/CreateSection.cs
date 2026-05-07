using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Backlinks;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Sections;

public record CreateSection(int DocumentId, int? ParentId, string Title, int Position) : IRequest<int>;

public class CreateSectionHandler : IRequestHandler<CreateSection, int>
{
    private readonly IAppDbContext _db;
    private readonly IDocumentLinkRefresher _links;

    public CreateSectionHandler(IAppDbContext db, IDocumentLinkRefresher links)
    {
        _db = db;
        _links = links;
    }

    public async Task<int> Handle(CreateSection request, CancellationToken cancellationToken)
    {
        if (request.ParentId is int pid)
        {
            var parentDocId = await _db.Sections
                .Where(s => s.Id == pid)
                .Select(s => (int?)s.DocumentId)
                .FirstOrDefaultAsync(cancellationToken);
            if (parentDocId is null)
                throw new NotFoundException($"Parent section {pid} not found");
            if (parentDocId != request.DocumentId)
                throw new ValidationException($"Parent section {pid} belongs to a different document");
        }

        var section = new Section
        {
            DocumentId = request.DocumentId,
            ParentId = request.ParentId,
            Title = request.Title,
            Position = request.Position
        };

        _db.Sections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);
        await _links.RefreshAsync(request.DocumentId, cancellationToken);
        return section.Id;
    }
}
