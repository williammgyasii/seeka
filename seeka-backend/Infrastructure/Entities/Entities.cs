using Microsoft.AspNetCore.Identity;
using Seeka.Domain.Assessments;

namespace Seeka.Infrastructure.Entities;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Workspace
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public Guid PersonalOwnerUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public List<WorkspaceMembership> Memberships { get; set; } = [];
}

public sealed class WorkspaceMembership
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Workspace Workspace { get; set; } = null!;
    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public WorkspaceRole Role { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public enum WorkspaceRole
{
    Owner,
    Member
}

public sealed class CandidateProfile
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string FullName { get; set; } = "";
    public string CountryOfResidence { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public string Seniority { get; set; } = "";
    public int YearsOfExperience { get; set; }
    public string ExperienceSummary { get; set; } = "";
    public string[] Skills { get; set; } = [];
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class JobPreference
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string[] TargetJobTitles { get; set; } = [];
    public string[] EmploymentTypes { get; set; } = [];
    public string AvailableWorkingHours { get; set; } = "";
    public decimal? MinimumCompensation { get; set; }
    public string? CompensationCurrency { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Domain { get; set; }
}

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

public enum JobAvailabilityStatus
{
    Active,
    Closed,
    Unverified
}

public sealed class JobSourceRecord
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    public string Provider { get; set; } = "manual";
    public string? ExternalId { get; set; }
    public string SourceUrl { get; set; } = "";
    public string CanonicalUrl { get; set; } = "";
    public string OriginalPayload { get; set; } = "";
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastCheckedAt { get; set; }
}

public sealed class EligibilityAssessment
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public EligibilityStatus Status { get; set; }
    public string OriginalRestrictionText { get; set; } = "";
    public string SourceUrl { get; set; } = "";
    public string[] EvidenceExcerpts { get; set; } = [];
    public string DecisionReason { get; set; } = "";
    public AssessmentOrigin Origin { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
}

public sealed class MatchAssessment
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public string ModelVersion { get; set; } = "";
    public int TotalScore { get; set; }
    public string ComponentsJson { get; set; } = "[]";
    public string[] MissingRequirements { get; set; } = [];
    public DateTimeOffset AssessedAt { get; set; }
}

public sealed class SavedJob
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    public PipelineStage Stage { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public enum PipelineStage
{
    Saved,
    Researching,
    ReadyToContact,
    Contacted,
    Replied,
    Interviewing,
    Offer,
    Closed
}

public enum ApplicationStatus
{
    NotApplied,
    Applied,
    Rejected,
    Withdrawn,
    Accepted
}

public sealed class Contact
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ProfessionalProfileUrl { get; set; }
    public string? ProfessionalEmail { get; set; }
    public string? EmailSource { get; set; }
    public ContactVerificationStatus VerificationStatus { get; set; }
    public string RelevanceReason { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

public enum ContactVerificationStatus
{
    Verified,
    Unverified,
    CatchAll,
    Invalid,
    Unknown
}

public sealed class OutreachDraft
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid SavedJobId { get; set; }
    public Guid? ContactId { get; set; }
    public string Channel { get; set; } = "";
    public string? Subject { get; set; }
    public string Body { get; set; } = "";
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class PipelineActivity
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid SavedJobId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Type { get; set; } = "";
    public string Description { get; set; } = "";
    public string? IdempotencyKey { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}
