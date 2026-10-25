using Microsoft.EntityFrameworkCore;
using Seeka.Engines.Pipeline;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Jobs;
using Seeka.ResourceAccess.Mapping;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class PipelineAccess(SeekaDbContext db) : IPipelineAccess
{
    public async Task<IReadOnlyList<PipelineSummary>> ListAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var savedJobs = await db.SavedJobs.AsNoTracking()
            .Include(item => item.JobPosting)
            .Where(item => item.WorkspaceId == workspaceId)
            .OrderByDescending(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);

        var jobIds = savedJobs.Select(item => item.JobPostingId).ToArray();
        var eligibility = (await db.EligibilityAssessments.AsNoTracking()
                .Where(item => item.WorkspaceId == workspaceId && jobIds.Contains(item.JobPostingId))
                .OrderByDescending(item => item.CheckedAt)
                .ToListAsync(cancellationToken))
            .GroupBy(item => item.JobPostingId)
            .ToDictionary(group => group.Key, group => group.First());
        var matches = (await db.MatchAssessments.AsNoTracking()
                .Where(item => item.WorkspaceId == workspaceId && jobIds.Contains(item.JobPostingId))
                .OrderByDescending(item => item.AssessedAt)
                .ToListAsync(cancellationToken))
            .GroupBy(item => item.JobPostingId)
            .ToDictionary(group => group.Key, group => group.First());

        return savedJobs.Select(item => new PipelineSummary(
            item.Id,
            item.JobPosting.CompanyName,
            item.JobPosting.Title,
            item.Stage,
            item.ApplicationStatus,
            eligibility.GetValueOrDefault(item.JobPostingId)?.Status,
            matches.GetValueOrDefault(item.JobPostingId)?.TotalScore,
            item.UpdatedAt)).ToArray();
    }

    public async Task<JobDetailResponse?> FindDetailAsync(
        Guid workspaceId,
        Guid savedJobId,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.AsNoTracking()
            .Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
        if (savedJob is null) return null;

        var eligibility = await db.EligibilityAssessments.AsNoTracking()
            .Where(item =>
                item.WorkspaceId == workspaceId &&
                item.JobPostingId == savedJob.JobPostingId)
            .OrderByDescending(item => item.CheckedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var match = await db.MatchAssessments.AsNoTracking()
            .Where(item =>
                item.WorkspaceId == workspaceId &&
                item.JobPostingId == savedJob.JobPostingId)
            .OrderByDescending(item => item.AssessedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var contacts = await db.Contacts.AsNoTracking()
            .Where(item =>
                item.WorkspaceId == workspaceId &&
                item.CompanyId == savedJob.JobPosting.CompanyId)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var drafts = await db.OutreachDrafts.AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId && item.SavedJobId == savedJobId)
            .OrderBy(item => item.CreatedAt)
            .ToListAsync(cancellationToken);
        var activities = await db.PipelineActivities.AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId && item.SavedJobId == savedJobId)
            .OrderByDescending(item => item.OccurredAt)
            .ToListAsync(cancellationToken);

        return new JobDetailResponse(
            savedJob.ToView(),
            savedJob.JobPosting.ToView(),
            eligibility?.ToView(),
            match?.ToView(),
            contacts.Select(item => item.ToView()).ToArray(),
            drafts.Select(item => item.ToView()).ToArray(),
            activities.Select(item => item.ToView()).ToArray());
    }

    public async Task<bool> ChangeStageAsync(
        Guid workspaceId,
        Guid savedJobId,
        PipelineStage stage,
        ActivityEntry activity,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
            cancellationToken);
        if (savedJob is null) return false;
        savedJob.Stage = stage;
        savedJob.UpdatedAt = activity.OccurredAt;
        db.PipelineActivities.Add(activity.ToRow(workspaceId, savedJobId));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AppendActivityAsync(
        Guid workspaceId,
        Guid savedJobId,
        ActivityEntry activity,
        CancellationToken cancellationToken)
    {
        var exists = await db.SavedJobs.AnyAsync(
            item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
            cancellationToken);
        if (!exists) return false;
        db.PipelineActivities.Add(activity.ToRow(workspaceId, savedJobId));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
