using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class OutreachDraft
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid SavedJobId { get; set; }
    public Guid? ContactId { get; set; }
    public string Channel { get; set; } = "";
    public string? Subject { get; set; }
    public string Body { get; set; } = "";
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
