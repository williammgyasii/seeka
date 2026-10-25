using Seeka.Engines.Eligibility;
using Seeka.Engines.Pipeline;

namespace Seeka.Managers.Contracts.Jobs;

public sealed record PipelineSummary(
    Guid SavedJobId,
    string CompanyName,
    string Title,
    PipelineStage Stage,
    ApplicationStatus ApplicationStatus,
    EligibilityStatus? EligibilityStatus,
    int? MatchScore,
    DateTimeOffset UpdatedAt);
