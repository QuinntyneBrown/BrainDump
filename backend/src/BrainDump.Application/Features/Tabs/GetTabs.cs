using System.Text.Json;
using BrainDump.Application.DTOs;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Tabs;

public record GetTabs() : IRequest<TabStateDto>;

public class GetTabsHandler : IRequestHandler<GetTabs, TabStateDto>
{
    private static readonly TabStateDto Empty = new(new[] { new TabPaneDto(Array.Empty<int>(), -1) });

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTabsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TabStateDto> Handle(GetTabs request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId) return Empty;

        var row = await _db.UserTabStates
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (row is null) return Empty;

        var stored = JsonSerializer.Deserialize<TabStateDto>(row.PanesJson, JsonOpts.Default) ?? Empty;

        // Drop references to documents that have been deleted since the
        // state was persisted (per L2-039 #3) and clamp activeIndex to a
        // valid value.
        var liveIds = await _db.Documents
            .AsNoTracking()
            .Select(d => d.Id)
            .ToListAsync(ct);
        var live = new HashSet<int>(liveIds);

        var cleanedPanes = stored.Panes
            .Select(p =>
            {
                var keptTabs = p.Tabs.Where(live.Contains).ToList();
                var clampedActive = keptTabs.Count == 0
                    ? -1
                    : Math.Clamp(p.ActiveIndex, 0, keptTabs.Count - 1);
                return new TabPaneDto(keptTabs, clampedActive);
            })
            .ToList();

        return new TabStateDto(cleanedPanes.Count == 0 ? Empty.Panes : cleanedPanes);
    }
}

internal static class JsonOpts
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web);
}
