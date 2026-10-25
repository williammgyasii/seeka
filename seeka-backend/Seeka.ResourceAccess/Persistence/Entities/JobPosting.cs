using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class JobPosting
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public string CompanyName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string OriginalUrl { get; set; } = "";
    public string CanonicalUrl { get; set; } = "";
    public string? LocationText { get; set; }
    public string? RestrictionText { get; set; }
    public string? EmploymentType { get; set; }
    public string[] RequiredSkills { get; set; } = [];
    public string? Seniority { get; set; }
    public string? WorkingHours { get; set; }
    public decimal? SalaryMinimum { get; set; }
    public decimal? SalaryMaximum { get; set; }
    public string? SalaryCurrency { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastCheckedAt { get; set; }
    public JobAvailabilityStatus AvailabilityStatus { get; set; } =
        JobAvailabilityStatus.Unverified;
}
