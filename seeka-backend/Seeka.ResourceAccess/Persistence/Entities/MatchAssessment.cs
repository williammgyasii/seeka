using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class MatchAssessment
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public string ModelVersion { get; set; } = "";
    public int TotalScore { get; set; }
    public string ComponentsJson { get; set; } = "[]";
    public string[] MissingRequirements { get; set; } = [];
    public DateTimeOffset AssessedAt { get; set; }
}
