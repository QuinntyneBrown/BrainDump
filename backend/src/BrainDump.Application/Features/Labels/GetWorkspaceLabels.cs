using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Labels;

public record GetWorkspaceLabels() : IRequest<IReadOnlyList<string>>;

public class GetWorkspaceLabelsHandler : IRequestHandler<GetWorkspaceLabels, IReadOnlyList<string>>
{
    private readonly IAppDbContext _db;

    public GetWorkspaceLabelsHandler(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<string>> Handle(GetWorkspaceLabels request, CancellationToken ct)
    {
        return await _db.Labels
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => l.Name)
            .ToListAsync(ct);
    }
}
