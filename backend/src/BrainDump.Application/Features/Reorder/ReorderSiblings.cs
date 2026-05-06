using BrainDump.Application.DTOs;
using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Reorder;

public record ReorderSiblings(List<ReorderItem> Items) : IRequest<Unit>;

public class ReorderSiblingsHandler : IRequestHandler<ReorderSiblings, Unit>
{
    private readonly IAppDbContext _db;

    public ReorderSiblingsHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(ReorderSiblings request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
            return Unit.Value;

        // Validate no duplicate positions in the request
        var positions = request.Items.Select(i => i.Position).ToList();
        if (positions.Distinct().Count() != positions.Count)
            throw new ValidationException("Duplicate positions are not allowed in a reorder request");

        var ids = request.Items.Select(i => i.Id).ToList();
        if (ids.Distinct().Count() != ids.Count)
            throw new ValidationException("Duplicate ids are not allowed in a reorder request");

        // Try to load as sections first
        var sections = await _db.Sections
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (sections.Count == ids.Count)
        {
            var firstParent = sections[0].ParentId;
            if (sections.Any(s => s.ParentId != firstParent))
                throw new ValidationException("All sections in a reorder request must share the same parent");

            foreach (var item in request.Items)
            {
                var section = sections.First(s => s.Id == item.Id);
                section.Position = item.Position;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        // Otherwise load as facts
        var facts = await _db.Facts
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(cancellationToken);

        if (facts.Count == ids.Count)
        {
            var firstSection = facts[0].SectionId;
            if (facts.Any(f => f.SectionId != firstSection))
                throw new ValidationException("All facts in a reorder request must share the same section");

            foreach (var item in request.Items)
            {
                var fact = facts.First(f => f.Id == item.Id);
                fact.Position = item.Position;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        throw new NotFoundException("One or more items in the reorder request were not found");
    }
}
