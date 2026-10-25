using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class PipelineActivity
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid SavedJobId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
