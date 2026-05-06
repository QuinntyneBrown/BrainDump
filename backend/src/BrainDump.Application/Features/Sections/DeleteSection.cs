using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Sections;

public record DeleteSection(int Id) : IRequest<Unit>;

public class DeleteSectionHandler : IRequestHandler<DeleteSection, Unit>
{
    private readonly IAppDbContext _db;

    public DeleteSectionHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteSection request, CancellationToken cancellationToken)
    {
        var root = await _db.Sections.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Section {request.Id} not found");

        // SQL Server can't ON DELETE CASCADE a self-reference, so we walk the
        // descendant subtree in code and remove sections breadth-first. The
        // section→fact FK still cascades at the DB level.
        var ids = new HashSet<int> { root.Id };
        var frontier = new Queue<int>();
        frontier.Enqueue(root.Id);
        while (frontier.Count > 0)
        {
            var parentId = frontier.Dequeue();
            var childIds = await _db.Sections
                .Where(s => s.ParentId == parentId)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);
            foreach (var childId in childIds)
            {
                if (ids.Add(childId)) frontier.Enqueue(childId);
            }
        }

        var sections = await _db.Sections
            .Where(s => ids.Contains(s.Id))
            .ToListAsync(cancellationToken);
        _db.Sections.RemoveRange(sections);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
