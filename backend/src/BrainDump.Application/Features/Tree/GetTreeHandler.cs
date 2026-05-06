using BrainDump.Application.DTOs;
using BrainDump.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Tree;

public class GetTreeHandler : IRequestHandler<GetTreeQuery, TreeDto>
{
    private readonly IAppDbContext _db;

    public GetTreeHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<TreeDto> Handle(GetTreeQuery request, CancellationToken cancellationToken)
    {
        var sections = await _db.Sections
            .AsNoTracking()
            .OrderBy(s => s.Position)
            .Select(s => new SectionDto(s.Id, s.ParentId, s.Title, s.Position))
            .ToListAsync(cancellationToken);

        var facts = await _db.Facts
            .AsNoTracking()
            .OrderBy(f => f.Position)
            .Select(f => new FactDto(f.Id, f.SectionId, f.Text, f.Position))
            .ToListAsync(cancellationToken);

        return new TreeDto(sections, facts);
    }
}
