using BrainDump.Application.Exceptions;
using BrainDump.Application.Interfaces;
using BrainDump.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BrainDump.Application.Features.Facts;

public record CreateFact(int SectionId, string Text, int Position) : IRequest<int>;

public class CreateFactHandler : IRequestHandler<CreateFact, int>
{
    private readonly IAppDbContext _db;

    public CreateFactHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(CreateFact request, CancellationToken cancellationToken)
    {
        var sectionExists = await _db.Sections.AnyAsync(s => s.Id == request.SectionId, cancellationToken);
        if (!sectionExists)
            throw new ValidationException($"Section {request.SectionId} does not exist");

        var fact = new Fact
        {
            SectionId = request.SectionId,
            Text = request.Text,
            Position = request.Position
        };

        _db.Facts.Add(fact);
        await _db.SaveChangesAsync(cancellationToken);
        return fact.Id;
    }
}
