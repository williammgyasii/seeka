using Seeka.Engines.Profiles;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Candidates;

/// <summary>Builds the candidate profile one onboarding step at a time, then finishes onboarding.</summary>
public sealed class CandidateProfileManager(
    IMembershipAccess memberships,
    ICandidateAccess candidates,
    IJobTitleAccess jobTitles,
    TimeProvider timeProvider)
{
    private const int MaxTitleResults = 20;
    private const int MaxQueryLength = 80;

    public async Task<CandidateProfileResponse> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var workspaceId = await WorkspaceAsync(userId, cancellationToken);
        return View(await candidates.FindProfileAsync(workspaceId, cancellationToken));
    }

    public async Task<ProfileSaveResult> SaveStepAsync(
        Guid userId,
        ProfileStep step,
        CancellationToken cancellationToken)
    {
        var titles = step is PreferencesStep ? await jobTitles.LoadAsync(cancellationToken) : null;
        var tidy = Tidy(step, titles);
        var errors = Check(tidy, titles);
        if (errors.Count > 0) return ProfileSaveResult.Rejected(errors);

        var workspaceId = await WorkspaceAsync(userId, cancellationToken);
        var stored = await candidates.FindProfileAsync(workspaceId, cancellationToken);
        await candidates.SaveStepAsync(
            workspaceId,
            tidy,
            OnboardingSteps.Mark(stored.CompletedSteps, tidy.Name),
            timeProvider.GetUtcNow(),
            cancellationToken);
        return ProfileSaveResult.Saved(View(await candidates.FindProfileAsync(workspaceId, cancellationToken)));
    }

    /// <summary>Field errors keyed by the missing step, or none once onboarding is finished.</summary>
    public async Task<IReadOnlyDictionary<string, string[]>> CompleteOnboardingAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspaceId = await WorkspaceAsync(userId, cancellationToken);
        var stored = await candidates.FindProfileAsync(workspaceId, cancellationToken);
        var missing = OnboardingSteps.Missing(stored.CompletedSteps);
        if (missing.Length > 0)
            return missing.ToDictionary(step => step, step => new[] { $"Save the {step} step before finishing." });

        await candidates.CompleteOnboardingAsync(workspaceId, timeProvider.GetUtcNow(), cancellationToken);
        return new Dictionary<string, string[]>();
    }

    public async Task<IReadOnlyList<string>> SearchJobTitlesAsync(string? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length > MaxQueryLength) return [];
        return (await jobTitles.LoadAsync(cancellationToken)).Search(query, MaxTitleResults);
    }

    public static PayBandOptions GetPayBands() => new(PayBands.ByCurrency, PayBands.DefaultByCountry);

    private async Task<Guid> WorkspaceAsync(Guid userId, CancellationToken cancellationToken) =>
        (await memberships.RequireWorkspaceAsync(userId, cancellationToken)).WorkspaceId;

    private static ProfileStep Tidy(ProfileStep step, JobTitleCatalog? titles) => step switch
    {
        AboutStep about => new AboutStep(CandidateProfilePolicy.Tidy(about.About)),
        ExperienceStep experience => new ExperienceStep(experience.Experience.Select(CandidateProfilePolicy.Tidy).ToArray()),
        EducationStep education => new EducationStep(
            education.Education.Select(CandidateProfilePolicy.Tidy).ToArray(),
            education.Certifications.Select(CandidateProfilePolicy.Tidy).ToArray()),
        ProjectsStep projects => new ProjectsStep(
            projects.Projects.Select(CandidateProfilePolicy.Tidy).ToArray(),
            projects.Volunteering.Select(CandidateProfilePolicy.Tidy).ToArray()),
        PreferencesStep preferences => new PreferencesStep(PreferencesPolicy.Tidy(preferences.Preferences, titles!)),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step.Name, "Unknown onboarding step.")
    };

    private static IReadOnlyDictionary<string, string[]> Check(ProfileStep step, JobTitleCatalog? titles) => step switch
    {
        AboutStep about => CandidateProfilePolicy.CheckAbout(about.About),
        ExperienceStep experience => CandidateProfilePolicy.CheckExperience(experience.Experience),
        EducationStep education => CandidateProfilePolicy.CheckEducation(education.Education, education.Certifications),
        ProjectsStep projects => CandidateProfilePolicy.CheckProjects(projects.Projects, projects.Volunteering),
        PreferencesStep preferences => PreferencesPolicy.Check(preferences.Preferences, titles!),
        _ => throw new ArgumentOutOfRangeException(nameof(step), step.Name, "Unknown onboarding step.")
    };

    private CandidateProfileResponse View(StoredProfile stored) =>
        new(
            stored.About,
            stored.Details.Experience,
            stored.Details.Education,
            stored.Details.Certifications,
            stored.Details.Projects,
            stored.Details.Volunteering,
            stored.Preferences,
            stored.CompletedSteps,
            stored.OnboardingCompletedAt is not null,
            ExperienceYears.Total(
                stored.Details.Experience,
                DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)));
}
