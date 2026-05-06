using BrainDump.Application.DTOs;
using BrainDump.Application.Exceptions;
using BrainDump.Application.Features.Reorder;
using BrainDump.Domain.Entities;
using BrainDump.UnitTests.TestDoubles;
using Xunit;

namespace BrainDump.UnitTests.Reorder;

public class ReorderSiblingsHandlerTests
{
    [Fact]
    public async Task ReorderSiblings_rejects_duplicate_positions()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        db.Sections.AddRange(
            new Section { Id = 1, ParentId = null, Title = "A", Position = 0 },
            new Section { Id = 2, ParentId = null, Title = "B", Position = 1 });
        await db.SaveChangesAsync();

        var handler = new ReorderSiblingsHandler(db);
        var items = new List<ReorderItem>
        {
            new(1, 0),
            new(2, 0)
        };

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new ReorderSiblings(items), CancellationToken.None));
    }

    [Fact]
    public async Task ReorderSiblings_rejects_sections_with_different_parents()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        db.Sections.AddRange(
            new Section { Id = 1, ParentId = null, Title = "A", Position = 0 },
            new Section { Id = 2, ParentId = 99, Title = "B", Position = 0 });
        await db.SaveChangesAsync();

        var handler = new ReorderSiblingsHandler(db);
        var items = new List<ReorderItem> { new(1, 0), new(2, 1) };

        await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new ReorderSiblings(items), CancellationToken.None));
    }

    [Fact]
    public async Task ReorderSiblings_updates_positions_for_valid_request()
    {
        await using var db = TestAppDbContext.CreateInMemory();
        db.Sections.AddRange(
            new Section { Id = 1, ParentId = null, Title = "A", Position = 0 },
            new Section { Id = 2, ParentId = null, Title = "B", Position = 1 });
        await db.SaveChangesAsync();

        var handler = new ReorderSiblingsHandler(db);
        var items = new List<ReorderItem> { new(1, 1), new(2, 0) };

        await handler.Handle(new ReorderSiblings(items), CancellationToken.None);

        Assert.Equal(1, db.Sections.Find(1)!.Position);
        Assert.Equal(0, db.Sections.Find(2)!.Position);
    }
}
