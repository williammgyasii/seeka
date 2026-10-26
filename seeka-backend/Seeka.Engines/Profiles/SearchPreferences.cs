namespace Seeka.Engines.Profiles;

/// <summary>What the candidate is looking for: roles, terms, and monthly pay.</summary>
public sealed record SearchPreferences(
    string[] TargetJobTitles,
    string[] EmploymentTypes,
    decimal? MinimumCompensation,
    decimal? MaximumCompensation,
    string? CompensationCurrency);
