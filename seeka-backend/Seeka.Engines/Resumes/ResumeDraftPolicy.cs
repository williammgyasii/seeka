using Seeka.Engines.Intake;

namespace Seeka.Engines.Resumes;

/// <summary>
/// Bounds what extracted facts can put into the onboarding form. The extractor is
/// not trusted to follow the schema's intent, only its shape.
/// </summary>
public static class ResumeDraftPolicy
{
    public const int MaxSkills = 30;
    public const int MaxTargetJobTitles = 10;
    public const int MaxSummaryLength = 2_000;
    public const int MaxItemLength = 80;
    public const int MaxYearsOfExperience = 60;

    public static ResumeDraft From(ResumeFacts facts) =>
        new(
            Bounded(facts.FullName, 160),
            Bounded(facts.Seniority, MaxItemLength),
            facts.YearsOfExperience is >= 0 and <= MaxYearsOfExperience ? facts.YearsOfExperience : null,
            Bounded(facts.ExperienceSummary, MaxSummaryLength),
            List(facts.Skills, MaxSkills),
            List(facts.TargetJobTitles, MaxTargetJobTitles));

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
