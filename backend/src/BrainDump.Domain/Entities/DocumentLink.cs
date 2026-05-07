namespace BrainDump.Domain.Entities;

public class DocumentLink
{
    public int SourceDocumentId { get; set; }
    public int TargetDocumentId { get; set; }
    public Document Source { get; set; } = null!;
    public Document Target { get; set; } = null!;
}
