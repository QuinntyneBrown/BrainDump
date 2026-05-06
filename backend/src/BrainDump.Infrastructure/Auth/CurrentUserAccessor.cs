using BrainDump.Application.Interfaces;

namespace BrainDump.Infrastructure.Auth;

public class CurrentUserAccessor : ICurrentUser
{
    public int? UserId { get; set; }
    public string? Email { get; set; }
}
