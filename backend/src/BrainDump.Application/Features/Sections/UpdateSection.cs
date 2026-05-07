using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Backlinks;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Sections;

public record UpdateSection(int Id, int? ParentId, string Title, int Position) : IRequest<Unit>;

public class UpdateSectionHandler : IRequestHandler<UpdateSection, Unit>
{
    private readonly IAppDbContext _db;
    private readonly IDocumentLinkRefresher _links;

    public UpdateSectionHandler(IAppDbContext db, IDocumentLinkRefresher links)
    {
        _db = db;
        _links = links;
    }

    public async Task<Unit> Handle(UpdateSection request, CancellationToken cancellationToken)
    {
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Section {request.Id} not found");

        section.ParentId = request.ParentId;
        section.Title = request.Title;
        section.Position = request.Position;

        await _db.SaveChangesAsync(cancellationToken);
        await _links.RefreshAsync(section.DocumentId, cancellationToken);
        return Unit.Value;
    }
}
