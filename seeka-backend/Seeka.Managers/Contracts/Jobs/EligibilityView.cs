using Seeka.Engines.Eligibility;

namespace Seeka.Managers.Contracts.Jobs;

public sealed record EligibilityView(
    Guid Id,
    Guid WorkspaceId,
    Guid JobPostingId,
    EligibilityStatus Status,
    string OriginalRestrictionText,
    string SourceUrl,
    string[] EvidenceExcerpts,
    string DecisionReason,
    AssessmentOrigin Origin,
    DateTimeOffset CheckedAt);
