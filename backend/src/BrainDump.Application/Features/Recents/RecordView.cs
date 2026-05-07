using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Recents;

public record RecordView(int DocumentId) : IRequest<Unit>;

public class RecordViewHandler : IRequestHandler<RecordView, Unit>
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public RecordViewHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RecordView request, CancellationToken ct)
    {
        if (_currentUser.UserId is not int userId)
            throw new ValidationException("No authenticated user");

        if (!await _db.Documents.AsNoTracking().AnyAsync(d => d.Id == request.DocumentId, ct))
            throw new NotFoundException($"Document {request.DocumentId} not found");

        var existing = await _db.UserDocumentViews
            .FirstOrDefaultAsync(v => v.UserId == userId && v.DocumentId == request.DocumentId, ct);

        var now = DateTime.UtcNow;
        if (existing is null)
        {
            _db.UserDocumentViews.Add(new UserDocumentView
            {
                UserId = userId,
                DocumentId = request.DocumentId,
                ViewedAt = now,
            });
        }
        else
        {
            existing.ViewedAt = now;
        }

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
