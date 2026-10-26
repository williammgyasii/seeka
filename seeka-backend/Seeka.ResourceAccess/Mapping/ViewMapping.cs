using Seeka.Engines.Pipeline;
using Seeka.Engines.Profiles;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Candidates;
using Seeka.Managers.Contracts.Jobs;
using Seeka.Managers.Contracts.Outreach;
using Seeka.Managers.Contracts.Pipeline;
using Seeka.ResourceAccess.Persistence.Entities;

namespace Seeka.ResourceAccess.Mapping;

internal static class ViewMapping
{
    public static CandidateProfileView ToView(this CandidateProfile item) =>
        new(item.Id, item.WorkspaceId, item.FullName, item.CountryOfResidence, item.TimeZone,
            SeniorityLevel.FromLegacy(item.Seniority) ?? "", item.YearsOfExperience, item.ExperienceSummary, item.Skills,
            item.UpdatedAt);

    public static JobPreferencesView ToView(this JobPreference item) =>
        new(item.Id, item.WorkspaceId, item.TargetJobTitles, item.EmploymentTypes,
            item.MinimumCompensation, item.CompensationCurrency, item.UpdatedAt);

    public static SearchPreferences ToPreferences(this JobPreference item) =>
        new(item.TargetJobTitles, item.EmploymentTypes,
            item.MinimumCompensation, item.MaximumCompensation, item.CompensationCurrency);

    public static SavedJobView ToView(this SavedJob item) =>
        new(item.Id, item.WorkspaceId, item.JobPostingId, item.Stage, item.ApplicationStatus,
            item.Notes, item.CreatedAt, item.UpdatedAt);

    public static JobPostingView ToView(this JobPosting item) =>
        new(item.Id, item.CompanyId, item.CompanyName, item.Title, item.Description,
            item.OriginalUrl, item.CanonicalUrl, item.LocationText, item.RestrictionText,
            item.EmploymentType, item.RequiredSkills, item.Seniority, item.WorkingHours,
            item.SalaryMinimum, item.SalaryMaximum, item.SalaryCurrency, item.PostedAt,
            item.FirstSeenAt, item.LastCheckedAt, item.AvailabilityStatus.ToString());

    public static EligibilityView ToView(this EligibilityAssessment item) =>
        new(item.Id, item.WorkspaceId, item.JobPostingId, item.Status,
            item.OriginalRestrictionText, item.SourceUrl, item.EvidenceExcerpts,
            item.DecisionReason, item.Origin, item.CheckedAt);

    public static MatchView ToView(this MatchAssessment item) =>
        new(item.Id, item.WorkspaceId, item.JobPostingId, item.ModelVersion, item.TotalScore,
            item.ComponentsJson, item.MissingRequirements, item.AssessedAt);

    public static ContactView ToView(this Contact item) =>
        new(item.Id, item.WorkspaceId, item.CompanyId, item.Name, item.Title,
            item.ProfessionalProfileUrl, item.ProfessionalEmail, item.EmailSource,
            item.VerificationStatus, item.RelevanceReason, item.CreatedAt);

    public static OutreachDraftView ToView(this OutreachDraft item) =>
        new(item.Id, item.WorkspaceId, item.SavedJobId, item.ContactId, item.Channel,
            item.Subject, item.Body, item.IsApproved, item.CreatedAt, item.UpdatedAt);

    public static ActivityView ToView(this PipelineActivity item) =>
        new(item.Id, item.WorkspaceId, item.SavedJobId, item.ActorUserId, item.Type,
            item.Description, item.IdempotencyKey, item.OccurredAt);

    public static PipelineActivity ToRow(
        this ActivityEntry entry,
        Guid workspaceId,
        Guid savedJobId) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SavedJobId = savedJobId,
            ActorUserId = entry.ActorUserId,
            Type = entry.Type,
            Description = entry.Description,
            IdempotencyKey = entry.IdempotencyKey,
            OccurredAt = entry.OccurredAt
        };
}
