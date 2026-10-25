namespace Seeka.Engines.Eligibility;

public sealed record EligibilityResult(
    EligibilityStatus Status,
    string Reason,
    IReadOnlyList<EligibilityEvidence> Evidence,
    AssessmentOrigin Origin,
    DateTimeOffset CheckedAt);
