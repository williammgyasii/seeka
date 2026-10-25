using Microsoft.AspNetCore.Identity;
using Seeka.Engines.Pipeline;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class SavedJob
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    public PipelineStage Stage { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
