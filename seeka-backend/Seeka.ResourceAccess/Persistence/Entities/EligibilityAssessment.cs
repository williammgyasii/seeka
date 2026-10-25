using Microsoft.AspNetCore.Identity;
using Seeka.Engines.Eligibility;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class EligibilityAssessment
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public EligibilityStatus Status { get; set; }
    public string OriginalRestrictionText { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string[] EvidenceExcerpts { get; set; } = [];
    public string DecisionReason { get; set; } = "";
    public AssessmentOrigin Origin { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}
