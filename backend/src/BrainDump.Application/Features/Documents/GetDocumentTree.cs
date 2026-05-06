using BrainDump.Application.DTOs;
using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Documents;

public record GetDocumentTree(int DocumentId) : IRequest<TreeDto>;

public class GetDocumentTreeHandler : IRequestHandler<GetDocumentTree, TreeDto>
{
    private readonly IAppDbContext _db;

    public GetDocumentTreeHandler(IAppDbContext db) => _db = db;

    public async Task<TreeDto> Handle(GetDocumentTree request, CancellationToken cancellationToken)
    {
        var documentExists = await _db.Documents
            .AsNoTracking()
            .AnyAsync(d => d.Id == request.DocumentId, cancellationToken);
        if (!documentExists)
            throw new NotFoundException($"Document {request.DocumentId} not found");

        var sections = await _db.Sections
            .AsNoTracking()
            .Where(s => s.DocumentId == request.DocumentId)
            .OrderBy(s => s.Position)
            .Select(s => new SectionDto(s.Id, s.ParentId, s.Title, s.Position))
            .ToListAsync(cancellationToken);

        var sectionIds = sections.Select(s => s.Id).ToList();
        var facts = await _db.Facts
            .AsNoTracking()
            .Where(f => sectionIds.Contains(f.SectionId))
            .OrderBy(f => f.Position)
            .Select(f => new FactDto(f.Id, f.SectionId, f.Text, f.Position))
            .ToListAsync(cancellationToken);

        return new TreeDto(sections, facts);
    }
}
