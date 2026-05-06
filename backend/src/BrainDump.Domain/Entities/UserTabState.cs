namespace BrainDump.Domain.Entities;

public class UserTabState
{
    public int UserId { get; set; }
    public string PanesJson { get; set; } = string.Empty;
    public User User { get; set; } = null!;
}
