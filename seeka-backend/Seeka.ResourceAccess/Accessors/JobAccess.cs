using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Seeka.Engines.Matching;
using Seeka.Engines.Pipeline;
using Seeka.Managers.Access.Jobs;
using Seeka.ResourceAccess.Mapping;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class JobAccess(SeekaDbContext db) : IJobAccess
{
    public async Task<ImportTarget> FindImportTargetAsync(
        string canonicalUrl,
        string companyName,
        string? companyDomain,
        CancellationToken cancellationToken)
    {
        var job = await db.JobPostings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.CanonicalUrl == canonicalUrl, cancellationToken);
        var company = await FindCompany(companyName, companyDomain)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        return new ImportTarget(
            job is null
                ? null
                : new ExistingJob(
                    job.CompanyName,
                    new JobSnapshot(
                        job.Title,
                        job.RequiredSkills,
                        job.Seniority,
                        job.WorkingHours,
                        job.SalaryMinimum,
                        job.SalaryMaximum,
                        job.SalaryCurrency)),
            company?.Name);
    }

    public async Task<ImportedJob> SaveImportAsync(
        JobImport import,
        CancellationToken cancellationToken)
    {
        var posting = import.Posting;
        var now = import.ImportedAt;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var company = await FindCompany(posting.CompanyName, posting.CompanyDomain)
            .FirstOrDefaultAsync(cancellationToken);
        if (company is null)
        {
            company = new Company
            {
                Id = Guid.NewGuid(),
                Name = posting.CompanyName,
                Domain = posting.CompanyDomain
            };
            db.Companies.Add(company);
        }

        var job = await db.JobPostings
            .SingleOrDefaultAsync(item => item.CanonicalUrl == posting.CanonicalUrl, cancellationToken);
        if (job is null)
        {
            job = new JobPosting
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                CompanyName = company.Name,
                Title = posting.Title,
                Description = posting.Description,
                OriginalUrl = posting.OriginalUrl,
                CanonicalUrl = posting.CanonicalUrl,
                LocationText = posting.LocationText,
                RestrictionText = posting.RestrictionText,
                EmploymentType = posting.EmploymentType,
                RequiredSkills = posting.RequiredSkills,
                Seniority = posting.Seniority,
                WorkingHours = posting.WorkingHours,
                SalaryMinimum = posting.SalaryMinimum,
                SalaryMaximum = posting.SalaryMaximum,
                SalaryCurrency = posting.SalaryCurrency,
                PostedAt = posting.PostedAt,
                FirstSeenAt = now,
                LastCheckedAt = now,
                AvailabilityStatus = JobAvailabilityStatus.Unverified
            };
            db.JobPostings.Add(job);
            db.JobSourceRecords.Add(new JobSourceRecord
            {
                Id = Guid.NewGuid(),
                JobPostingId = job.Id,
                Provider = "manual",
                SourceUrl = posting.OriginalUrl,
                CanonicalUrl = posting.CanonicalUrl,
                OriginalPayload = posting.OriginalPayload,
                FirstSeenAt = now,
                LastCheckedAt = now
            });
        }
        else
        {
            job.LastCheckedAt = now;
        }

        var savedJob = await db.SavedJobs.SingleOrDefaultAsync(
            item => item.WorkspaceId == import.WorkspaceId && item.JobPostingId == job.Id,
            cancellationToken);
        if (savedJob is null)
        {
            savedJob = new SavedJob
            {
                Id = Guid.NewGuid(),
                WorkspaceId = import.WorkspaceId,
                JobPostingId = job.Id,
                Stage = PipelineStage.Saved,
                ApplicationStatus = ApplicationStatus.NotApplied,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.SavedJobs.Add(savedJob);
        }

        db.EligibilityAssessments.Add(new EligibilityAssessment
        {
            Id = Guid.NewGuid(),
            WorkspaceId = import.WorkspaceId,
            JobPostingId = job.Id,
            Status = import.Eligibility.Status,
            OriginalRestrictionText = import.OriginalRestrictionText,
            SourceUrl = posting.OriginalUrl,
            EvidenceExcerpts = import.Eligibility.Evidence.Select(item => item.Excerpt).ToArray(),
            DecisionReason = import.Eligibility.Reason,
            Origin = import.Eligibility.Origin,
            CheckedAt = import.Eligibility.CheckedAt
        });
        db.MatchAssessments.Add(new MatchAssessment
        {
            Id = Guid.NewGuid(),
            WorkspaceId = import.WorkspaceId,
            JobPostingId = job.Id,
            ModelVersion = import.Match.ModelVersion,
            TotalScore = import.Match.TotalScore,
            ComponentsJson = JsonSerializer.Serialize(import.Match.Components),
            MissingRequirements = import.Match.MissingRequirements.ToArray(),
            AssessedAt = now
        });
        db.PipelineActivities.Add(import.Activity.ToRow(import.WorkspaceId, savedJob.Id));

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new ImportedJob(savedJob.Id, job.Id);
    }

    private IQueryable<Company> FindCompany(string companyName, string? companyDomain) =>
        companyDomain is null
            ? db.Companies.Where(item => item.Name == companyName)
            : db.Companies.Where(item => item.Domain == companyDomain);
}
