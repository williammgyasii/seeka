using Seeka.Engines.Intake;
using Seeka.Engines.Profiles;

namespace Seeka.Engines.Resumes;

/// <summary>
/// Bounds what extracted facts can put into the onboarding form. The extractor is
/// not trusted to follow the schema's intent, only its shape. Bad items are
/// dropped and bad values blanked, never rejected, because the user did not
/// type them.
/// </summary>
public static class ResumeDraftPolicy
{
    public const int MaxSkills = 30;
    public const int MaxTargetJobTitles = 10;
    public const int MaxSummaryLength = 2_000;
    public const int MaxItemLength = 80;
    public const int MaxYearsOfExperience = 60;

    /// <param name="today">
    /// When given, years of experience come from the job dates where any are dated,
    /// instead of the extractor's own count.
    /// </param>
    public static ResumeDraft From(ResumeFacts facts, DateOnly? today = null)
    {
        var details = facts.Details is { } reported ? Clean(reported) : ProfileDetails.Empty;
        var reportedYears = facts.YearsOfExperience is >= 0 and <= MaxYearsOfExperience ? facts.YearsOfExperience : null;
        var datedYears = today is { } date ? ExperienceYears.Total(details.Experience, date) : null;
        return new ResumeDraft(
            Bounded(facts.FullName, ProfileLimits.MaxTextLength),
            SeniorityLevel.Exact(facts.Seniority),
            datedYears is <= MaxYearsOfExperience ? datedYears : reportedYears,
            Bounded(facts.ExperienceSummary, MaxSummaryLength),
            List(facts.Skills, MaxSkills),
            List(facts.TargetJobTitles, MaxTargetJobTitles))
        {
            Headline = Bounded(facts.Headline, ProfileLimits.MaxHeadlineLength),
            Details = details
        };
    }

    private static ProfileDetails Clean(ProfileDetails details) =>
        new(
            Items(details.Experience, ProfileLimits.MaxExperience, CleanJob),
            Items(details.Education, ProfileLimits.MaxEducation, CleanSchool),
            Items(details.Certifications, ProfileLimits.MaxCertifications, CleanCertification),
            Items(details.Languages, ProfileLimits.MaxLanguages, CleanLanguage),
            Items(details.Links, ProfileLimits.MaxLinks, CleanLink),
            Items(details.Projects, ProfileLimits.MaxProjects, CleanProject),
            Items(details.Volunteering, ProfileLimits.MaxVolunteering, CleanVolunteering));

    private static WorkExperience? CleanJob(WorkExperience job) =>
        Text(job.Company) is { } company && Text(job.Title) is { } title
            ? new WorkExperience(
                company,
                title,
                Text(job.Location),
                Month(job.StartMonth),
                job.Current ? null : Month(job.EndMonth),
                job.Current,
                (job.Highlights ?? [])
                    .Select(highlight => Bounded(highlight, ProfileLimits.MaxHighlightLength))
                    .OfType<string>()
                    .Take(ProfileLimits.MaxHighlights)
                    .ToArray())
            : null;

    private static Education? CleanSchool(Education school) =>
        Text(school.School) is { } name
            ? new Education(
                name,
                Text(school.Degree),
                Text(school.FieldOfStudy),
                Year(school.StartYear),
                school.EndYear >= school.StartYear || school.StartYear is null ? Year(school.EndYear) : null)
            : null;

    private static Certification? CleanCertification(Certification certification) =>
        Text(certification.Name) is { } name
            ? new Certification(name, Text(certification.Issuer), Year(certification.Year))
            : null;

    private static LanguageSkill? CleanLanguage(LanguageSkill language) =>
        Text(language.Language) is { } name ? new LanguageSkill(name, ProfileLimits.Level(language.Level)) : null;

    private static ProfileLink? CleanLink(ProfileLink link) =>
        Https(link.Url) is { } url ? new ProfileLink(Text(link.Label), url) : null;

    private static Project? CleanProject(Project project) =>
        Text(project.Name) is { } name
            ? new Project(name, Bounded(project.Description, ProfileLimits.MaxDescriptionLength), Https(project.Url))
            : null;

    private static Volunteering? CleanVolunteering(Volunteering role) =>
        Text(role.Organization) is { } organization
            ? new Volunteering(
                organization,
                Text(role.Role),
                Month(role.StartMonth),
                role.Current ? null : Month(role.EndMonth),
                role.Current,
                Bounded(role.Description, ProfileLimits.MaxDescriptionLength))
            : null;

    private static T[] Items<T>(IEnumerable<T>? items, int maxCount, Func<T, T?> clean) where T : class =>
        (items ?? []).Select(clean).OfType<T>().Take(maxCount).ToArray();

    private static string? Text(string? value) => Bounded(value, ProfileLimits.MaxTextLength);

    private static string? Month(string? value) =>
        ProfileLimits.MonthIndex(value?.Trim()) is null ? null : value!.Trim();

    private static int? Year(int? value) => ProfileLimits.IsYear(value) ? value : null;

    private static string? Https(string? value) =>
        ProfileLimits.IsHttps(value?.Trim()) ? value!.Trim() : null;

    private static string? Bounded(string? value, int maxLength) =>
        IntakeRules.OptionalText(value) is { } text
            ? text[..Math.Min(text.Length, maxLength)]
            : null;

    private static string[] List(IReadOnlyList<string>? values, int maxCount) =>
        IntakeRules.CleanList(values ?? [])
            .Where(value => value.Length <= MaxItemLength)
            .Take(maxCount)
            .ToArray();
}
