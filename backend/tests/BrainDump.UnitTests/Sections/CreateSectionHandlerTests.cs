using BrainDump.Application.Features.Sections;
using BrainDump.UnitTests.TestDoubles;
using Xunit;

namespace BrainDump.UnitTests.Sections;

public class CreateSectionHandlerTests
{
    [Fact]
    public async Task CreateSection_persists_section_and_returns_new_id()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        var handler = new CreateSectionHandler(db);

        var id = await handler.Handle(
            new CreateSection(ParentId: null, Title: "Inbox", Position: 0),
            CancellationToken.None);

        Assert.NotEqual(0, id);

        var stored = Assert.Single(db.Sections);
        Assert.Equal(id, stored.Id);
        Assert.Equal("Inbox", stored.Title);
        Assert.Equal(0, stored.Position);
        Assert.Null(stored.ParentId);
    }

    [Fact]
    public async Task CreateSection_with_parent_id_records_the_parent_relationship()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        var handler = new CreateSectionHandler(db);

        var parentId = await handler.Handle(new CreateSection(null, "Root", 0), CancellationToken.None);
        var childId = await handler.Handle(new CreateSection(parentId, "Child", 0), CancellationToken.None);

        var child = db.Sections.Find(childId);
        Assert.NotNull(child);
        Assert.Equal(parentId, child!.ParentId);
    }
}
