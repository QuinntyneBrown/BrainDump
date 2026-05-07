using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Labels;

public record SetDocumentLabels(int DocumentId, IReadOnlyList<string> Labels) : IRequest<Unit>;

public class SetDocumentLabelsHandler : IRequestHandler<SetDocumentLabels, Unit>
{
    private const int MaxLabelLength = 64;

    private readonly IAppDbContext _db;

    public SetDocumentLabelsHandler(IAppDbContext db) => _db = db;

    public async Task<Unit> Handle(SetDocumentLabels request, CancellationToken ct)
    {
        if (!await _db.Documents.AsNoTracking().AnyAsync(d => d.Id == request.DocumentId, ct))
            throw new NotFoundException($"Document {request.DocumentId} not found");

        // Normalize: trim, drop empties, lower-case for de-dup, cap length.
        var requested = request.Labels
            .Select(s => s.Trim().TrimStart('#'))
            .Where(s => s.Length > 0)
            .Select(s => s.Length > MaxLabelLength ? s[..MaxLabelLength] : s)
            .GroupBy(s => s.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        // Look up existing labels by lower-cased name (covers Sqlite's
        // case-sensitive default; SQL Server's CI collation handles it
        // natively).
        var lowered = requested.Select(s => s.ToLowerInvariant()).ToList();
        var existing = await _db.Labels
            .Where(l => lowered.Contains(l.Name.ToLower()))
            .ToListAsync(ct);

        var existingByLower = existing
            .GroupBy(l => l.Name.ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        var nameToLabelId = new Dictionary<string, int>();
        foreach (var name in requested)
        {
            if (existingByLower.TryGetValue(name.ToLowerInvariant(), out var match))
            {
                nameToLabelId[name] = match.Id;
            }
            else
            {
                var fresh = new Label { Name = name };
                _db.Labels.Add(fresh);
                await _db.SaveChangesAsync(ct);
                nameToLabelId[name] = fresh.Id;
                existingByLower[name.ToLowerInvariant()] = fresh;
            }
        }

        var desiredLabelIds = nameToLabelId.Values.ToHashSet();

        var currentPairs = await _db.DocumentLabels
            .Where(dl => dl.DocumentId == request.DocumentId)
            .ToListAsync(ct);

        var currentLabelIds = currentPairs.Select(p => p.LabelId).ToHashSet();

        var toRemove = currentPairs.Where(p => !desiredLabelIds.Contains(p.LabelId)).ToList();
        var toAdd = desiredLabelIds
            .Where(id => !currentLabelIds.Contains(id))
            .Select(id => new DocumentLabel { DocumentId = request.DocumentId, LabelId = id })
            .ToList();

        if (toRemove.Count > 0) _db.DocumentLabels.RemoveRange(toRemove);
        if (toAdd.Count > 0) _db.DocumentLabels.AddRange(toAdd);

        if (toAdd.Count > 0 || toRemove.Count > 0)
            await _db.SaveChangesAsync(ct);

        return Unit.Value;
    }
}
