using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Facts;

public record DeleteFact(int Id) : IRequest<Unit>;

public class DeleteFactHandler : IRequestHandler<DeleteFact, Unit>
{
    private readonly IAppDbContext _db;

    public DeleteFactHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<Unit> Handle(DeleteFact request, CancellationToken cancellationToken)
    {
        var fact = await _db.Facts.FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Fact {request.Id} not found");

        _db.Facts.Remove(fact);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
