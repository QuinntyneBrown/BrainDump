using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Backlinks;

public record BacklinkDto(int Id, string Title, string Excerpt);

public record GetBacklinks(int DocumentId) : IRequest<IReadOnlyList<BacklinkDto>>;

public class GetBacklinksHandler : IRequestHandler<GetBacklinks, IReadOnlyList<BacklinkDto>>
{
    private const int ExcerptMaxLength = 120;

    private readonly IAppDbContext _db;

    public GetBacklinksHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<BacklinkDto>> Handle(GetBacklinks request, CancellationToken ct)
    {
        // Inner-join document so deleted sources drop out.
        var sources = await (
            from dl in _db.DocumentLinks.AsNoTracking()
            where dl.TargetDocumentId == request.DocumentId
            join d in _db.Documents.AsNoTracking() on dl.SourceDocumentId equals d.Id
            orderby d.Title
            select new { d.Id, d.Title }
        ).ToListAsync(ct);

        if (sources.Count == 0) return Array.Empty<BacklinkDto>();

        // Pull the first matching fact text for each source as the excerpt.
        // For a single-editor workspace this is a small N x N + a few row
        // reads — fine without a more elaborate query.
        var sourceIds = sources.Select(s => s.Id).ToList();
        var firstFacts = await _db.Facts
            .AsNoTracking()
            .Where(f => sourceIds.Contains(f.Section.DocumentId))
            .OrderBy(f => f.Section.DocumentId).ThenBy(f => f.Position)
            .Select(f => new { DocumentId = f.Section.DocumentId, f.Text })
            .ToListAsync(ct);

        var excerptByDoc = firstFacts
            .GroupBy(f => f.DocumentId)
            .ToDictionary(
                g => g.Key,
                g => Truncate(g.First().Text));

        return sources
            .Select(s => new BacklinkDto(
                s.Id,
                s.Title,
                excerptByDoc.TryGetValue(s.Id, out var snippet) ? snippet : Truncate(s.Title)))
            .ToList();
    }

    private static string Truncate(string s) =>
        s.Length <= ExcerptMaxLength ? s : s[..ExcerptMaxLength] + "…";
}
