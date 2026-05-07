using BrainDump.Application.DTOs;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Recents;

public record GetRecentDocuments(int Limit) : IRequest<IReadOnlyList<RecentDocumentDto>>;

public class GetRecentDocumentsHandler : IRequestHandler<GetRecentDocuments, IReadOnlyList<RecentDocumentDto>>
{
    private const int DefaultLimit = 10;
    private const int MaxLimit = 50;

    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetRecentDocumentsHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RecentDocumentDto>> Handle(
        GetRecentDocuments request,
        CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId) return Array.Empty<RecentDocumentDto>();

        var limit = request.Limit <= 0 ? DefaultLimit : Math.Min(request.Limit, MaxLimit);

        // INNER JOIN against document drops rows whose target was deleted
        // (per L2-047 #4) — the FK has ON DELETE CASCADE so the row also
        // gets removed, but the join keeps reads safe even mid-cascade.
        var recents = await (
            from v in _db.UserDocumentViews.AsNoTracking()
            join d in _db.Documents.AsNoTracking() on v.DocumentId equals d.Id
            where v.UserId == userId
            orderby v.ViewedAt descending
            select new RecentDocumentDto(d.Id, d.Title, v.ViewedAt)
        ).Take(limit).ToListAsync(ct);

        return recents;
    }
}
