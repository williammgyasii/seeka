using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Seeka.Engines.Profiles;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Contracts.Candidates;
using Seeka.ResourceAccess.Mapping;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class CandidateAccess(SeekaDbContext db) : ICandidateAccess
{
    public async Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        return profile is null || preferences is null
            ? null
            : new CandidateSetupResponse(profile.ToView(), preferences.ToView());
    }

    public async Task<StoredProfile> FindProfileAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        if (profile is null) return StoredProfile.Empty with { Preferences = preferences?.ToPreferences() };

        var details = Details(profile);
        return new StoredProfile(
            new AboutYou(
                profile.FullName,
                profile.Headline,
                profile.CountryOfResidence,
                profile.TimeZone,
                SeniorityLevel.FromLegacy(profile.Seniority),
                profile.YearsOfExperience,
                NullIfEmpty(profile.ExperienceSummary),
                profile.Skills,
                details.Languages,
                details.Links),
            details,
            preferences?.ToPreferences(),
            profile.CompletedSteps,
            profile.OnboardingCompletedAt);
    }

    public async Task SaveStepAsync(
        Guid workspaceId,
        ProfileStep step,
        string[] completedSteps,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        if (profile is null)
        {
            profile = new CandidateProfile { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
            db.CandidateProfiles.Add(profile);
        }

        var details = Details(profile);
        switch (step)
        {
            case AboutStep { About: var about }:
                profile.FullName = about.FullName;
                profile.Headline = about.Headline;
                profile.CountryOfResidence = about.CountryOfResidence;
                profile.TimeZone = about.TimeZone;
                profile.Seniority = about.Seniority ?? "";
                profile.YearsOfExperience = about.YearsOfExperience;
                profile.ExperienceSummary = about.ExperienceSummary ?? "";
                profile.Skills = about.Skills;
                details = details with { Languages = about.Languages, Links = about.Links };
                break;
            case ExperienceStep experience:
                details = details with { Experience = experience.Experience };
                break;
            case EducationStep education:
                details = details with { Education = education.Education, Certifications = education.Certifications };
                break;
            case ProjectsStep projects:
                details = details with { Projects = projects.Projects, Volunteering = projects.Volunteering };
                break;
            case PreferencesStep preferences:
                await SavePreferencesAsync(workspaceId, preferences.Preferences, updatedAt, cancellationToken);
                break;
        }

        profile.DetailsJson = JsonSerializer.Serialize(details);
        profile.CompletedSteps = completedSteps;
        profile.UpdatedAt = updatedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CompleteOnboardingAsync(
        Guid workspaceId,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles
            .SingleAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        profile.OnboardingCompletedAt ??= completedAt;
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SavePreferencesAsync(
        Guid workspaceId,
        SearchPreferences step,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var preferences = await db.JobPreferences
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        if (preferences is null)
        {
            preferences = new JobPreference { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
            db.JobPreferences.Add(preferences);
        }

        preferences.TargetJobTitles = step.TargetJobTitles;
        preferences.EmploymentTypes = step.EmploymentTypes;
        preferences.MinimumCompensation = step.MinimumCompensation;
        preferences.MaximumCompensation = step.MaximumCompensation;
        preferences.CompensationCurrency = step.CompensationCurrency;
        preferences.UpdatedAt = updatedAt;
    }

    private static ProfileDetails Details(CandidateProfile profile) =>
        profile.DetailsJson is null
            ? ProfileDetails.Empty
            : JsonSerializer.Deserialize<ProfileDetails>(profile.DetailsJson) ?? ProfileDetails.Empty;

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
}
