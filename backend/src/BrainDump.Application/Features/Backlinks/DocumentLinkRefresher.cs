using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Backlinks;

public class DocumentLinkRefresher : IDocumentLinkRefresher
{
    private readonly IAppDbContext _db;

    public DocumentLinkRefresher(IAppDbContext db) => _db = db;

    public async Task RefreshAsync(int sourceDocumentId, CancellationToken ct = default)
    {
        // Pull every text source on the document in two queries: section
        // titles and fact text. Both can carry wiki-links.
        var sectionTitles = await _db.Sections
            .AsNoTracking()
            .Where(s => s.DocumentId == sourceDocumentId)
            .Select(s => s.Title)
            .ToListAsync(ct);

        var factTexts = await _db.Facts
            .AsNoTracking()
            .Where(f => f.Section.DocumentId == sourceDocumentId)
            .Select(f => f.Text)
            .ToListAsync(ct);

        var links = WikiLinkParser
            .ExtractMany(sectionTitles.Cast<string?>().Concat(factTexts.Cast<string?>()))
            .ToList();

        // Resolve titles to ids in a single round-trip.
        var titles = links.OfType<ParsedLink.ByTitle>().Select(l => l.Title).Distinct().ToList();
        var docsByTitle = titles.Count == 0
            ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            : await _db.Documents
                .AsNoTracking()
                .Where(d => titles.Contains(d.Title))
                .OrderBy(d => d.Id)
                .GroupBy(d => d.Title)
                .Select(g => new { Title = g.Key, Id = g.Min(d => d.Id) })
                .ToDictionaryAsync(x => x.Title, x => x.Id, StringComparer.OrdinalIgnoreCase, ct);

        // Verify the existence of any explicit-id links — drop those that
        // don't resolve, per L2-043 #3 and the design note about
        // unresolved references.
        var idCandidates = links.OfType<ParsedLink.ById>().Select(l => l.TargetId).Distinct().ToList();
        var liveIds = idCandidates.Count == 0
            ? new HashSet<int>()
            : (await _db.Documents
                .AsNoTracking()
                .Where(d => idCandidates.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync(ct)).ToHashSet();

        var resolvedTargets = new HashSet<int>();
        foreach (var link in links)
        {
            switch (link)
            {
                case ParsedLink.ById byId when liveIds.Contains(byId.TargetId) && byId.TargetId != sourceDocumentId:
                    resolvedTargets.Add(byId.TargetId);
                    break;
                case ParsedLink.ByTitle byTitle when docsByTitle.TryGetValue(byTitle.Title, out var id) && id != sourceDocumentId:
                    resolvedTargets.Add(id);
                    break;
            }
        }

        // Replace the existing row set for this source.
        var existing = await _db.DocumentLinks
            .Where(dl => dl.SourceDocumentId == sourceDocumentId)
            .ToListAsync(ct);
        var existingTargets = existing.Select(e => e.TargetDocumentId).ToHashSet();

        var toRemove = existing.Where(e => !resolvedTargets.Contains(e.TargetDocumentId)).ToList();
        var toAdd = resolvedTargets
            .Where(id => !existingTargets.Contains(id))
            .Select(id => new DocumentLink { SourceDocumentId = sourceDocumentId, TargetDocumentId = id })
            .ToList();

        if (toRemove.Count > 0) _db.DocumentLinks.RemoveRange(toRemove);
        if (toAdd.Count > 0) _db.DocumentLinks.AddRange(toAdd);

        if (toAdd.Count > 0 || toRemove.Count > 0)
            await _db.SaveChangesAsync(ct);
    }
}
