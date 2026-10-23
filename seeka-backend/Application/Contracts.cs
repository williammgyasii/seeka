using Seeka.Domain.Assessments;
using Seeka.Infrastructure.Entities;

namespace Seeka.Application;

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

public sealed record CandidateSetupResponse(
    CandidateProfile Profile,
    JobPreference Preferences);

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

public sealed record AddContactRequest(
    string Name,
    string Title,
    string? ProfessionalProfileUrl,
    string? ProfessionalEmail,
    string RelevanceReason);

public sealed record UpdateDraftRequest(
    string? Subject,
    string Body,
    bool IsApproved);

public sealed record RecordActivityRequest(
    string Type,
    string Description,
    string? IdempotencyKey);

public sealed record JobDetailResponse(
    SavedJob SavedJob,
    JobPosting Job,
    EligibilityAssessment? Eligibility,
    MatchAssessment? Match,
    IReadOnlyList<Contact> Contacts,
    IReadOnlyList<OutreachDraft> Drafts,
    IReadOnlyList<PipelineActivity> Activities);

public sealed record SessionResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid WorkspaceId);

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName);

public sealed record LoginRequest(string Email, string Password);

public sealed record ErrorResponse(string Error);

public sealed record PipelineSummary(
    Guid SavedJobId,
    string CompanyName,
    string Title,
    PipelineStage Stage,
    ApplicationStatus ApplicationStatus,
    EligibilityStatus? EligibilityStatus,
    int? MatchScore,
    DateTimeOffset UpdatedAt);
