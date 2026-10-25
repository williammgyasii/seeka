namespace Seeka.Managers.Contracts.Jobs;

public sealed record JobPostingView(
    Guid Id,
    Guid? CompanyId,
    string CompanyName,
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
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastCheckedAt,
    string AvailabilityStatus);
