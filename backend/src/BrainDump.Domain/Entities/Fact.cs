namespace BrainDump.Domain.Entities;

public class Fact
{
    public int Id { get; set; }
    public int SectionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int Position { get; set; }
    public Section Section { get; set; } = null!;
}
