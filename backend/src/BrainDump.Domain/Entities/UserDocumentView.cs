namespace BrainDump.Domain.Entities;

public class UserDocumentView
{
    public int UserId { get; set; }
    public int DocumentId { get; set; }
    public DateTime ViewedAt { get; set; }
    public User User { get; set; } = null!;
    public Document Document { get; set; } = null!;
}
