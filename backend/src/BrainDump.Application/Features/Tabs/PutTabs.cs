using System.Text.Json;
using BrainDump.Application.DTOs;
using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Tabs;

public record PutTabs(TabStateDto State) : IRequest<Unit>;

public class PutTabsHandler : IRequestHandler<PutTabs, Unit>
{
    // Cap stored payload to bound parse cost (per L2-039 security note).
    private const int MaxJsonBytes = 16 * 1024;
    private const int MaxPanes = 2;

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public PutTabsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(PutTabs request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            throw new ValidationException("No authenticated user");
        if (request.State.Panes.Count is < 1 or > MaxPanes)
            throw new ValidationException($"State must have 1..{MaxPanes} panes");

        var json = JsonSerializer.Serialize(request.State, JsonOpts.Default);
        if (json.Length > MaxJsonBytes)
            throw new ValidationException("Tab state too large");

        var existing = await _db.UserTabStates.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (existing is null)
        {
            _db.UserTabStates.Add(new UserTabState { UserId = userId, PanesJson = json });
        }
        else
        {
            existing.PanesJson = json;
        }
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
