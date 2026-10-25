namespace Seeka.Managers.Contracts.Candidates;

public sealed record CandidateProfileView(
    Guid Id,
    Guid WorkspaceId,
    string FullName,
    string CountryOfResidence,
    string TimeZone,
    string Seniority,
    int YearsOfExperience,
    string ExperienceSummary,
    string[] Skills,
    DateTimeOffset UpdatedAt);
