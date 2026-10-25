using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid PersonalOwnerUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<WorkspaceMembership> Memberships { get; set; } = [];
}
