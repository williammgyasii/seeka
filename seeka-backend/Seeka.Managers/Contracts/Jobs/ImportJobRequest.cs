namespace Seeka.Managers.Contracts.Jobs;

public sealed record ImportJobRequest(
    string CompanyName,
    string? CompanyDomain,
    string Title,
    string Description,
    string SourceUrl,
    string? LocationText,
    string? RestrictionText,
    string? EmploymentType,
    string[] RequiredSkills,
    string? Seniority,
    string? WorkingHours,
    decimal? SalaryMinimum,
    decimal? SalaryMaximum,
    string? SalaryCurrency,
    DateTimeOffset? PostedAt);
