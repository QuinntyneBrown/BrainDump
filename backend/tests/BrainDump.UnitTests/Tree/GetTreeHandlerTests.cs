using BrainDump.Application.Features.Tree;
using BrainDump.Domain.Entities;
using BrainDump.UnitTests.TestDoubles;
using Xunit;

namespace BrainDump.UnitTests.Tree;

public class GetTreeHandlerTests
{
    [Fact]
    public async Task GetTree_returns_sections_and_facts_mapped_to_dtos_and_ordered_by_position()
    {
        await using var db = TestAppDbContext.CreateInMemory();

        var rootSection = new Section { Id = 1, ParentId = null, Title = "Root", Position = 0 };
        var childSection = new Section { Id = 2, ParentId = 1, Title = "Child", Position = 1 };
        db.Sections.AddRange(rootSection, childSection);

        db.Facts.AddRange(
            new Fact { Id = 10, SectionId = 1, Text = "Fact B", Position = 1 },
            new Fact { Id = 11, SectionId = 1, Text = "Fact A", Position = 0 });
        await db.SaveChangesAsync();

        var handler = new GetTreeHandler(db);
        var tree = await handler.Handle(new GetTreeQuery(), CancellationToken.None);

        Assert.Equal(2, tree.Sections.Count);
        Assert.Equal("Root", tree.Sections[0].Title);
        Assert.Equal(0, tree.Sections[0].Position);
        Assert.Equal("Child", tree.Sections[1].Title);
        Assert.Equal(1, tree.Sections[1].ParentId);

        Assert.Equal(2, tree.Facts.Count);
        Assert.Equal("Fact A", tree.Facts[0].Text);
        Assert.Equal(0, tree.Facts[0].Position);
        Assert.Equal("Fact B", tree.Facts[1].Text);
    }
}
