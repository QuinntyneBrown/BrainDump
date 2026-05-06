using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;

namespace BrainDump.Application.Features.Sections;

public record CreateSection(int? ParentId, string Title, int Position) : IRequest<int>;

public class CreateSectionHandler : IRequestHandler<CreateSection, int>
{
    private readonly IAppDbContext _db;

    public CreateSectionHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(CreateSection request, CancellationToken cancellationToken)
    {
        var section = new Section
        {
            ParentId = request.ParentId,
            Title = request.Title,
            Position = request.Position
        };

        _db.Sections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);
        return section.Id;
    }
}
