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
        var section = await _db.Sections.FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException($"Section {request.Id} not found");

        _db.Sections.Remove(section);
        await _db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
