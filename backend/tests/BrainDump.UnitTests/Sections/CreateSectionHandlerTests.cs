using BrainDump.Application.Features.Sections;
using BrainDump.Domain.Entities;
using BrainDump.UnitTests.TestDoubles;
using Xunit;

namespace BrainDump.UnitTests.Sections;

public class CreateSectionHandlerTests
{
    [Fact]
    public async Task CreateSection_persists_section_and_returns_new_id()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        var doc = SeedDocument(db);
        var handler = new CreateSectionHandler(db);

        var id = await handler.Handle(
            new CreateSection(DocumentId: doc.Id, ParentId: null, Title: "Inbox", Position: 0),
            CancellationToken.None);

        Assert.NotEqual(0, id);

        var stored = Assert.Single(db.Sections);
        Assert.Equal(id, stored.Id);
        Assert.Equal(doc.Id, stored.DocumentId);
        Assert.Equal("Inbox", stored.Title);
        Assert.Equal(0, stored.Position);
        Assert.Null(stored.ParentId);
    }

    [Fact]
    public async Task CreateSection_with_parent_id_records_the_parent_relationship()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        var doc = SeedDocument(db);
        var handler = new CreateSectionHandler(db);

        var parentId = await handler.Handle(new CreateSection(doc.Id, null, "Root", 0), CancellationToken.None);
        var childId = await handler.Handle(new CreateSection(doc.Id, parentId, "Child", 0), CancellationToken.None);

        var child = db.Sections.Find(childId);
        Assert.NotNull(child);
        Assert.Equal(parentId, child!.ParentId);
        Assert.Equal(doc.Id, child.DocumentId);
    }

    [Fact]
    public async Task CreateSection_rejects_parent_in_a_different_document()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        var docA = SeedDocument(db, "Doc A");
        var docB = SeedDocument(db, "Doc B");
        var handler = new CreateSectionHandler(db);

        var rootInA = await handler.Handle(
            new CreateSection(docA.Id, null, "Root A", 0), CancellationToken.None);

        await Assert.ThrowsAsync<BrainDump.Application.Exceptions.ValidationException>(() =>
            handler.Handle(new CreateSection(docB.Id, rootInA, "Cross-doc child", 0), CancellationToken.None));
    }

    private static Document SeedDocument(TestAppDbContext db, string title = "Doc")
    {
        var doc = new Document
        {
            Title = title,
            Position = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        db.Documents.Add(doc);
        db.SaveChanges();
        return doc;
    }
}
