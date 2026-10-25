using Microsoft.Extensions.Logging;
using Seeka.Engines.Eligibility;
using Seeka.Engines.Intake;
using Seeka.Engines.Matching;
using Seeka.Engines.Pipeline;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Jobs;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Candidates;
using Seeka.Managers.Contracts.Jobs;
using Seeka.Managers.Contracts.Pipeline;

namespace Seeka.Managers.JobSearch;

public sealed class JobSearchManager(
    IMembershipAccess memberships,
    ICandidateAccess candidates,
    IJobAccess jobs,
    IPipelineAccess pipeline,
    TimeProvider timeProvider,
    ILogger<JobSearchManager> logger)
{
    public async Task<JobDetailResponse> ImportAsync(
        Guid userId,
        ImportJobRequest request,
        CancellationToken cancellationToken)
    {
        var sourceUri = IntakeRules.RequireHttpUrl(request.SourceUrl, nameof(request.SourceUrl));
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var setup = await candidates.GetRequiredSetupAsync(workspace.WorkspaceId, cancellationToken);
        var now = timeProvider.GetUtcNow();

        var posting = NewPosting(request, sourceUri);
        var target = await jobs.FindImportTargetAsync(
            posting.CanonicalUrl,
            posting.CompanyName,
            posting.CompanyDomain,
            cancellationToken);
        var snapshot = target.Job?.Snapshot ?? new JobSnapshot(
            posting.Title,
            posting.RequiredSkills,
            posting.Seniority,
            posting.WorkingHours,
            posting.SalaryMinimum,
            posting.SalaryMaximum,
            posting.SalaryCurrency);
        var companyName = target.Job?.CompanyName
            ?? target.ExistingCompanyName
            ?? posting.CompanyName;

        var eligibility = EligibilityEngine.Assess(
            setup.Profile.CountryOfResidence,
            AssessmentText(request),
            sourceUri,
            timeProvider);
        var match = MatchEngine.Assess(Candidate(setup), snapshot);

        var imported = await jobs.SaveImportAsync(
            new JobImport(
                workspace.WorkspaceId,
                posting,
                eligibility,
                request.RestrictionText ?? request.LocationText ?? "",
                match,
                new ActivityEntry(
                    userId,
                    "job_imported",
                    $"Imported {snapshot.Title} at {companyName}.",
                    now),
                now),
            cancellationToken);
        logger.LogInformation(
            "Job {JobId} imported into workspace {WorkspaceId} with eligibility {EligibilityStatus} and match {MatchScore}",
            imported.JobPostingId,
            workspace.WorkspaceId,
            eligibility.Status,
            match.TotalScore);
        return await pipeline.FindDetailAsync(workspace.WorkspaceId, imported.SavedJobId, cancellationToken)
            ?? throw new InvalidOperationException("Imported job could not be reloaded.");
    }

    public async Task<IReadOnlyList<PipelineSummary>> GetPipelineAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await pipeline.ListAsync(workspace.WorkspaceId, cancellationToken);
    }

    public async Task<JobDetailResponse?> FindJobAsync(
        Guid userId,
        Guid savedJobId,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await pipeline.FindDetailAsync(workspace.WorkspaceId, savedJobId, cancellationToken);
    }

    public async Task<bool> ChangeStageAsync(
        Guid userId,
        Guid savedJobId,
        PipelineStage stage,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await pipeline.ChangeStageAsync(
            workspace.WorkspaceId,
            savedJobId,
            stage,
            new ActivityEntry(
                userId,
                "stage_changed",
                $"Pipeline stage changed to {stage}.",
                timeProvider.GetUtcNow()),
            cancellationToken);
    }

    public async Task<bool> RecordActivityAsync(
        Guid userId,
        Guid savedJobId,
        RecordActivityRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await pipeline.AppendActivityAsync(
            workspace.WorkspaceId,
            savedJobId,
            new ActivityEntry(
                userId,
                request.Type.Trim(),
                request.Description.Trim(),
                timeProvider.GetUtcNow(),
                request.IdempotencyKey?.Trim()),
            cancellationToken);
    }

    private static NewJobPosting NewPosting(ImportJobRequest request, Uri sourceUri) =>
        new(
            request.CompanyName.Trim(),
            request.CompanyDomain?.Trim().ToLowerInvariant(),
            request.Title.Trim(),
            request.Description.Trim(),
            sourceUri.ToString(),
            IntakeRules.Canonicalize(sourceUri),
            request.LocationText?.Trim(),
            request.RestrictionText?.Trim(),
            request.EmploymentType?.Trim(),
            IntakeRules.CleanList(request.RequiredSkills),
            request.Seniority?.Trim(),
            request.WorkingHours?.Trim(),
            request.SalaryMinimum,
            request.SalaryMaximum,
            IntakeRules.Currency(request.SalaryCurrency),
            request.PostedAt,
            request.Description);

    private static CandidateSnapshot Candidate(CandidateSetupResponse setup) =>
        new(
            setup.Preferences.TargetJobTitles,
            setup.Profile.Skills,
            setup.Profile.Seniority,
            setup.Profile.YearsOfExperience,
            setup.Profile.TimeZone,
            setup.Preferences.MinimumCompensation,
            setup.Preferences.CompensationCurrency);

    private static string AssessmentText(ImportJobRequest request) =>
        string.Join(
            "\n",
            new[] { request.LocationText, request.RestrictionText, request.Description }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
}
