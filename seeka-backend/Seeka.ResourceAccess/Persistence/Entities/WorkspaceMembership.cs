using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class WorkspaceMembership
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public WorkspaceRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
