namespace Seeka.Engines.Matching;

public sealed record JobSnapshot(
    string Title,
    IReadOnlyList<string> RequiredSkills,
    string? Seniority,
    string? WorkingHours,
    decimal? MinimumCompensation,
    decimal? MaximumCompensation,
    string? CompensationCurrency);
