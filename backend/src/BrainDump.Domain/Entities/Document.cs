namespace BrainDump.Domain.Entities;

public class Document
{
    public int Id { get; set; }
    public int? FolderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Folder? Folder { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}
