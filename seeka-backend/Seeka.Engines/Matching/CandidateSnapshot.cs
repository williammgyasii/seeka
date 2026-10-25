namespace Seeka.Engines.Matching;

public sealed record CandidateSnapshot(
    IReadOnlyList<string> TargetRoles,
    IReadOnlyList<string> Skills,
    string Seniority,
    int YearsOfExperience,
    string TimeZone,
    decimal? MinimumCompensation,
    string? CompensationCurrency);
