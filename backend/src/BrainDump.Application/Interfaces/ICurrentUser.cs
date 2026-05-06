namespace BrainDump.Application.Interfaces;

public interface ICurrentUser
{
    int? UserId { get; set; }
    string? Email { get; set; }
}
