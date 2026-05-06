namespace BrainDump.Domain.Entities;

public class Section
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int? ParentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public Document Document { get; set; } = null!;
    public Section? Parent { get; set; }
    public ICollection<Section> Children { get; set; } = new List<Section>();
    public ICollection<Fact> Facts { get; set; } = new List<Fact>();
}
