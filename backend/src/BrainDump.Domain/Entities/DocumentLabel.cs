namespace BrainDump.Domain.Entities;

public class DocumentLabel
{
    public int DocumentId { get; set; }
    public int LabelId { get; set; }
    public Document Document { get; set; } = null!;
    public Label Label { get; set; } = null!;
}
