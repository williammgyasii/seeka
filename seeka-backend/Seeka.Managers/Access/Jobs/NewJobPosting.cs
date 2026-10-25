namespace Seeka.Managers.Access.Jobs;

public sealed record NewJobPosting(
    string CompanyName,
    string? CompanyDomain,
    string Title,
    string Description,
    string OriginalUrl,
    string CanonicalUrl,
    string? LocationText,
    string? RestrictionText,
    string? EmploymentType,
    string[] RequiredSkills,
    string? Seniority,
    string? WorkingHours,
    decimal? SalaryMinimum,
    decimal? SalaryMaximum,
    string? SalaryCurrency,
    DateTimeOffset? PostedAt,
    string OriginalPayload);
