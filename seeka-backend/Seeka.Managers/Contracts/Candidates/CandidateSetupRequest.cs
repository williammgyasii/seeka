namespace Seeka.Managers.Contracts.Candidates;

public sealed record CandidateSetupRequest(
    string FullName,
    string CountryOfResidence,
    string TimeZone,
    string Seniority,
    int YearsOfExperience,
    string ExperienceSummary,
    string[] Skills,
    string[] TargetJobTitles,
    string[] EmploymentTypes,
    string AvailableWorkingHours,
    decimal? MinimumCompensation,
    string? CompensationCurrency);
