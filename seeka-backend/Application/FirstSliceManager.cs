using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Seeka.Domain.Assessments;
using Seeka.Domain.Outreach;
using Seeka.Infrastructure;
using Seeka.Infrastructure.Entities;

namespace Seeka.Application;

public sealed class FirstSliceManager(
    SeekaDbContext db,
    TimeProvider timeProvider,
    ILogger<FirstSliceManager> logger)
{
    public async Task<CandidateSetupResponse?> GetSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        return profile is null || preferences is null
            ? null
            : new CandidateSetupResponse(profile, preferences);
    }

    public async Task<CandidateSetupResponse> SaveSetupAsync(
        Guid workspaceId,
        CandidateSetupRequest request,
        CancellationToken cancellationToken)
    {
        ValidateSetup(request);
        var now = timeProvider.GetUtcNow();
        var profile = await db.CandidateProfiles
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);

        profile ??= new CandidateProfile { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
        preferences ??= new JobPreference { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
        if (db.Entry(profile).State == EntityState.Detached) db.CandidateProfiles.Add(profile);
        if (db.Entry(preferences).State == EntityState.Detached) db.JobPreferences.Add(preferences);

        profile.FullName = request.FullName.Trim();
        profile.CountryOfResidence = request.CountryOfResidence.Trim();
        profile.TimeZone = request.TimeZone.Trim();
        profile.Seniority = request.Seniority.Trim();
        profile.YearsOfExperience = request.YearsOfExperience;
        profile.ExperienceSummary = request.ExperienceSummary.Trim();
        profile.Skills = Clean(request.Skills);
        profile.UpdatedAt = now;

        preferences.TargetJobTitles = Clean(request.TargetJobTitles);
        preferences.EmploymentTypes = Clean(request.EmploymentTypes);
        preferences.AvailableWorkingHours = request.AvailableWorkingHours.Trim();
        preferences.MinimumCompensation = request.MinimumCompensation;
        preferences.CompensationCurrency = NormalizeCurrency(request.CompensationCurrency);
        preferences.UpdatedAt = now;

        await db.SaveChangesAsync(cancellationToken);
        return new CandidateSetupResponse(profile, preferences);
    }

    public async Task<JobDetailResponse> ImportJobAsync(
        Guid workspaceId,
        Guid actorUserId,
        ImportJobRequest request,
        CancellationToken cancellationToken)
    {
        var sourceUri = ValidateHttpUrl(request.SourceUrl, nameof(request.SourceUrl));
        var canonicalUrl = Canonicalize(sourceUri);
        var setup = await GetRequiredSetupAsync(workspaceId, cancellationToken);
        var now = timeProvider.GetUtcNow();

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var company = await FindOrCreateCompanyAsync(request, cancellationToken);
        var job = await db.JobPostings
            .SingleOrDefaultAsync(item => item.CanonicalUrl == canonicalUrl, cancellationToken);
        if (job is null)
        {
            job = new JobPosting
            {
                Id = Guid.NewGuid(),
                CompanyId = company.Id,
                CompanyName = company.Name,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                OriginalUrl = sourceUri.ToString(),
                CanonicalUrl = canonicalUrl,
                LocationText = request.LocationText?.Trim(),
                RestrictionText = request.RestrictionText?.Trim(),
                EmploymentType = request.EmploymentType?.Trim(),
                RequiredSkills = Clean(request.RequiredSkills),
                Seniority = request.Seniority?.Trim(),
                WorkingHours = request.WorkingHours?.Trim(),
                SalaryMinimum = request.SalaryMinimum,
                SalaryMaximum = request.SalaryMaximum,
                SalaryCurrency = NormalizeCurrency(request.SalaryCurrency),
                PostedAt = request.PostedAt,
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
                SourceUrl = sourceUri.ToString(),
                CanonicalUrl = canonicalUrl,
                OriginalPayload = request.Description,
                FirstSeenAt = now,
                LastCheckedAt = now
            });
        }
        else
        {
            job.LastCheckedAt = now;
        }

        var savedJob = await db.SavedJobs.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.JobPostingId == job.Id,
            cancellationToken);
        if (savedJob is null)
        {
            savedJob = new SavedJob
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                JobPostingId = job.Id,
                Stage = PipelineStage.Saved,
                ApplicationStatus = ApplicationStatus.NotApplied,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.SavedJobs.Add(savedJob);
        }

        var assessmentText = string.Join(
            "\n",
            new[] { request.LocationText, request.RestrictionText, request.Description }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        var eligibilityResult = EligibilityEngine.Assess(
            setup.Profile.CountryOfResidence,
            assessmentText,
            sourceUri,
            timeProvider);
        var eligibility = new EligibilityAssessment
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            JobPostingId = job.Id,
            Status = eligibilityResult.Status,
            OriginalRestrictionText = request.RestrictionText ?? request.LocationText ?? "",
            SourceUrl = sourceUri.ToString(),
            EvidenceExcerpts = eligibilityResult.Evidence.Select(item => item.Excerpt).ToArray(),
            DecisionReason = eligibilityResult.Reason,
            Origin = eligibilityResult.Origin,
            CheckedAt = eligibilityResult.CheckedAt
        };
        db.EligibilityAssessments.Add(eligibility);

        var matchResult = MatchEngine.Assess(
            new CandidateSnapshot(
                setup.Preferences.TargetJobTitles,
                setup.Profile.Skills,
                setup.Profile.Seniority,
                setup.Profile.YearsOfExperience,
                setup.Profile.TimeZone,
                setup.Preferences.MinimumCompensation,
                setup.Preferences.CompensationCurrency),
            new JobSnapshot(
                job.Title,
                job.RequiredSkills,
                job.Seniority,
                job.WorkingHours,
                job.SalaryMinimum,
                job.SalaryMaximum,
                job.SalaryCurrency));
        var match = new MatchAssessment
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            JobPostingId = job.Id,
            ModelVersion = matchResult.ModelVersion,
            TotalScore = matchResult.TotalScore,
            ComponentsJson = JsonSerializer.Serialize(matchResult.Components),
            MissingRequirements = matchResult.MissingRequirements.ToArray(),
            AssessedAt = now
        };
        db.MatchAssessments.Add(match);
        db.PipelineActivities.Add(Activity(
            workspaceId,
            savedJob.Id,
            actorUserId,
            "job_imported",
            $"Imported {job.Title} at {job.CompanyName}.",
            now));

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation(
            "Job {JobId} imported into workspace {WorkspaceId} with eligibility {EligibilityStatus} and match {MatchScore}",
            job.Id,
            workspaceId,
            eligibility.Status,
            match.TotalScore);
        return await GetJobDetailAsync(workspaceId, savedJob.Id, cancellationToken)
            ?? throw new InvalidOperationException("Imported job could not be reloaded.");
    }

    public async Task<IReadOnlyList<PipelineSummary>> GetPipelineAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var savedJobs = await db.SavedJobs.AsNoTracking()
            .Include(item => item.JobPosting)
            .Where(item => item.WorkspaceId == workspaceId)
            .OrderByDescending(item => item.UpdatedAt)
            .ToListAsync(cancellationToken);

        var jobIds = savedJobs.Select(item => item.JobPostingId).ToArray();
        var eligibility = await LatestEligibilityAsync(workspaceId, jobIds, cancellationToken);
        var matches = await LatestMatchesAsync(workspaceId, jobIds, cancellationToken);
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

    public async Task<JobDetailResponse?> GetJobDetailAsync(
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
            savedJob,
            savedJob.JobPosting,
            eligibility,
            match,
            contacts,
            drafts,
            activities);
    }

    public async Task<bool> UpdateStageAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid savedJobId,
        PipelineStage stage,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
            cancellationToken);
        if (savedJob is null) return false;
        savedJob.Stage = stage;
        savedJob.UpdatedAt = timeProvider.GetUtcNow();
        db.PipelineActivities.Add(Activity(
            workspaceId,
            savedJobId,
            actorUserId,
            "stage_changed",
            $"Pipeline stage changed to {stage}.",
            savedJob.UpdatedAt));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Contact?> AddContactAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid savedJobId,
        AddContactRequest request,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
        if (savedJob?.JobPosting.CompanyId is null) return null;
        var profileUrl = OptionalContactFields.NormalizeHttpUrl(
            request.ProfessionalProfileUrl);
        var now = timeProvider.GetUtcNow();
        var contact = new Contact
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            CompanyId = savedJob.JobPosting.CompanyId.Value,
            Name = request.Name.Trim(),
            Title = request.Title.Trim(),
            ProfessionalProfileUrl = profileUrl,
            ProfessionalEmail = OptionalContactFields.NormalizeText(
                request.ProfessionalEmail),
            EmailSource = "manual",
            VerificationStatus = ContactVerificationStatus.Unknown,
            RelevanceReason = request.RelevanceReason.Trim(),
            CreatedAt = now
        };
        db.Contacts.Add(contact);
        db.PipelineActivities.Add(Activity(
            workspaceId,
            savedJobId,
            actorUserId,
            "contact_added",
            $"Added {contact.Name} as a contact.",
            now));
        await db.SaveChangesAsync(cancellationToken);
        return contact;
    }

    public async Task<IReadOnlyList<OutreachDraft>?> GenerateDraftsAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid savedJobId,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        var setup = await GetRequiredSetupAsync(workspaceId, cancellationToken);
        var savedJob = await db.SavedJobs.Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
        var contact = await db.Contacts.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == contactId,
            cancellationToken);
        if (savedJob is null || contact is null ||
            contact.CompanyId != savedJob.JobPosting.CompanyId) return null;

        var now = timeProvider.GetUtcNow();
        var generated = OutreachEngine.Generate(new OutreachContext(
            setup.Profile.FullName,
            setup.Profile.CountryOfResidence,
            setup.Profile.TimeZone,
            setup.Profile.ExperienceSummary,
            savedJob.JobPosting.Title,
            savedJob.JobPosting.CompanyName,
            contact.Name));
        var drafts = generated.Select(item => new OutreachDraft
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SavedJobId = savedJobId,
            ContactId = contactId,
            Channel = item.Channel,
            Subject = item.Subject,
            Body = item.Body,
            CreatedAt = now,
            UpdatedAt = now
        }).ToArray();
        db.OutreachDrafts.AddRange(drafts);
        db.PipelineActivities.Add(Activity(
            workspaceId,
            savedJobId,
            actorUserId,
            "drafts_generated",
            "Generated four grounded outreach drafts for review.",
            now));
        await db.SaveChangesAsync(cancellationToken);
        return drafts;
    }

    public async Task<bool> UpdateDraftAsync(
        Guid workspaceId,
        Guid draftId,
        UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        var draft = await db.OutreachDrafts.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == draftId,
            cancellationToken);
        if (draft is null) return false;
        draft.Subject = request.Subject?.Trim();
        draft.Body = request.Body.Trim();
        draft.IsApproved = request.IsApproved;
        draft.UpdatedAt = timeProvider.GetUtcNow();
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RecordActivityAsync(
        Guid workspaceId,
        Guid actorUserId,
        Guid savedJobId,
        RecordActivityRequest request,
        CancellationToken cancellationToken)
    {
        var exists = await db.SavedJobs.AnyAsync(
            item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
            cancellationToken);
        if (!exists) return false;
        db.PipelineActivities.Add(Activity(
            workspaceId,
            savedJobId,
            actorUserId,
            request.Type.Trim(),
            request.Description.Trim(),
            timeProvider.GetUtcNow(),
            request.IdempotencyKey?.Trim()));
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<CandidateSetupResponse> GetRequiredSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken) =>
        await GetSetupAsync(workspaceId, cancellationToken)
        ?? throw new InvalidOperationException(
            "Complete the candidate profile and job preferences before importing a job.");

    private async Task<Company> FindOrCreateCompanyAsync(
        ImportJobRequest request,
        CancellationToken cancellationToken)
    {
        var domain = request.CompanyDomain?.Trim().ToLowerInvariant();
        var company = domain is null
            ? await db.Companies.FirstOrDefaultAsync(
                item => item.Name == request.CompanyName.Trim(),
                cancellationToken)
            : await db.Companies.SingleOrDefaultAsync(
                item => item.Domain == domain,
                cancellationToken);
        if (company is not null) return company;
        company = new Company
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName.Trim(),
            Domain = domain
        };
        db.Companies.Add(company);
        return company;
    }

    private async Task<Dictionary<Guid, EligibilityAssessment>> LatestEligibilityAsync(
        Guid workspaceId,
        Guid[] jobIds,
        CancellationToken cancellationToken) =>
        (await db.EligibilityAssessments.AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId && jobIds.Contains(item.JobPostingId))
            .OrderByDescending(item => item.CheckedAt)
            .ToListAsync(cancellationToken))
        .GroupBy(item => item.JobPostingId)
        .ToDictionary(group => group.Key, group => group.First());

    private async Task<Dictionary<Guid, MatchAssessment>> LatestMatchesAsync(
        Guid workspaceId,
        Guid[] jobIds,
        CancellationToken cancellationToken) =>
        (await db.MatchAssessments.AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId && jobIds.Contains(item.JobPostingId))
            .OrderByDescending(item => item.AssessedAt)
            .ToListAsync(cancellationToken))
        .GroupBy(item => item.JobPostingId)
        .ToDictionary(group => group.Key, group => group.First());

    private static PipelineActivity Activity(
        Guid workspaceId,
        Guid savedJobId,
        Guid actorUserId,
        string type,
        string description,
        DateTimeOffset now,
        string? idempotencyKey = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SavedJobId = savedJobId,
            ActorUserId = actorUserId,
            Type = type,
            Description = description,
            IdempotencyKey = idempotencyKey,
            OccurredAt = now
        };

    private static void ValidateSetup(CandidateSetupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.CountryOfResidence) ||
            string.IsNullOrWhiteSpace(request.TimeZone) ||
            request.TargetJobTitles.Length == 0)
            throw new ArgumentException("Name, residence, timezone, and a target role are required.");
        if (request.YearsOfExperience < 0)
            throw new ArgumentOutOfRangeException(nameof(request.YearsOfExperience));
        if (request.MinimumCompensation is not null &&
            string.IsNullOrWhiteSpace(request.CompensationCurrency))
            throw new ArgumentException("Compensation currency is required with a minimum.");
    }

    private static Uri ValidateHttpUrl(string value, string parameterName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Only absolute HTTP and HTTPS URLs are allowed.", parameterName);
        return uri;
    }

    private static string Canonicalize(Uri uri)
    {
        var builder = new UriBuilder(uri) { Fragment = "", Query = "" };
        return builder.Uri.ToString().TrimEnd('/');
    }

    private static string[] Clean(IEnumerable<string> values) =>
        values.Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string? NormalizeCurrency(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}
