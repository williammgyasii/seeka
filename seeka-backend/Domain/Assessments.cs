using System.Text.RegularExpressions;

namespace Seeka.Domain.Assessments;

public enum EligibilityStatus
{
    Eligible,
    NeedsConfirmation,
    Ineligible,
    Unknown
}

public enum AssessmentOrigin
{
    Rules,
    Ai,
    Manual
}

public enum Compatibility
{
    Compatible,
    PartiallyCompatible,
    Incompatible,
    Unknown
}

public sealed record EligibilityEvidence(string Excerpt, Uri SourceUrl);

public sealed record EligibilityResult(
    EligibilityStatus Status,
    string Reason,
    IReadOnlyList<EligibilityEvidence> Evidence,
    AssessmentOrigin Origin,
    DateTimeOffset CheckedAt);

public static partial class EligibilityEngine
{
    private static readonly string[] RestrictionMarkers =
    [
        "must reside",
        "must be based",
        "only",
        "within the united states",
        "within the us",
        "uk only",
        "us only"
    ];

    public static EligibilityResult Assess(
        string countryOfResidence,
        string postingText,
        Uri sourceUrl,
        TimeProvider? timeProvider = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryOfResidence);
        ArgumentException.ThrowIfNullOrWhiteSpace(postingText);

        var excerpts = Sentences()
            .Split(postingText)
            .Select(sentence => sentence.Trim())
            .Where(sentence => sentence.Length > 0)
            .ToArray();

        var inclusions = excerpts
            .Where(sentence => ContainsCountry(sentence, countryOfResidence))
            .ToArray();
        var restrictions = excerpts
            .Where(IsGeographicRestriction)
            .Where(sentence => !ContainsCountry(sentence, countryOfResidence))
            .ToArray();
        var evidence = inclusions
            .Concat(restrictions)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(excerpt => new EligibilityEvidence(excerpt, sourceUrl))
            .ToArray();

        var (status, reason) = (inclusions.Length > 0, restrictions.Length > 0) switch
        {
            (true, true) => (
                EligibilityStatus.NeedsConfirmation,
                "The posting contains conflicting location evidence that requires review."),
            (true, false) => (
                EligibilityStatus.Eligible,
                $"{countryOfResidence} is explicitly included and no conflicting restriction was found."),
            (false, true) => (
                EligibilityStatus.Ineligible,
                $"The posting contains a residence restriction that does not include {countryOfResidence}."),
            _ => (
                EligibilityStatus.Unknown,
                "Remote wording alone does not establish geographic hiring eligibility.")
        };

        return new EligibilityResult(
            status,
            reason,
            evidence,
            AssessmentOrigin.Rules,
            (timeProvider ?? TimeProvider.System).GetUtcNow());
    }

    private static bool ContainsCountry(string text, string country) =>
        text.Contains(country, StringComparison.OrdinalIgnoreCase);

    private static bool IsGeographicRestriction(string text) =>
        RestrictionMarkers.Any(marker =>
            text.Contains(marker, StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex Sentences();
}

public sealed record CandidateSnapshot(
    IReadOnlyList<string> TargetRoles,
    IReadOnlyList<string> Skills,
    string Seniority,
    int YearsOfExperience,
    string TimeZone,
    decimal? MinimumCompensation,
    string? CompensationCurrency);

public sealed record JobSnapshot(
    string Title,
    IReadOnlyList<string> RequiredSkills,
    string? Seniority,
    string? WorkingHours,
    decimal? MinimumCompensation,
    decimal? MaximumCompensation,
    string? CompensationCurrency);

public sealed record MatchComponent(
    string Name,
    int EarnedPoints,
    int AvailablePoints,
    Compatibility Compatibility,
    string Explanation);

public sealed record MatchResult(
    string ModelVersion,
    int TotalScore,
    IReadOnlyList<MatchComponent> Components,
    IReadOnlyList<string> MissingRequirements);

public static class MatchEngine
{
    public static MatchResult Assess(CandidateSnapshot candidate, JobSnapshot job)
    {
        var missingSkills = job.RequiredSkills
            .Where(required => !candidate.Skills.Any(skill => SameMeaning(skill, required)))
            .ToArray();
        var matchedSkillCount = job.RequiredSkills.Count - missingSkills.Length;
        var skillsRatio = job.RequiredSkills.Count == 0
            ? 1m
            : (decimal)matchedSkillCount / job.RequiredSkills.Count;

        var roleAligned = candidate.TargetRoles.Any(role => SameMeaning(role, job.Title));
        var seniorityKnown = !string.IsNullOrWhiteSpace(job.Seniority);
        var seniorityAligned = seniorityKnown &&
            string.Equals(candidate.Seniority, job.Seniority, StringComparison.OrdinalIgnoreCase);
        var hoursKnown = !string.IsNullOrWhiteSpace(job.WorkingHours);
        var hoursAligned = hoursKnown && IsUtcCompatible(candidate.TimeZone, job.WorkingHours!);
        var compensation = AssessCompensation(candidate, job);

        var components = new[]
        {
            Component("Target role alignment", roleAligned ? 25 : 10, 25,
                roleAligned ? Compatibility.Compatible : Compatibility.PartiallyCompatible,
                roleAligned ? "The job title aligns with a target role." : "The title is adjacent to, but not an exact target role."),
            Component("Relevant experience", candidate.YearsOfExperience > 0 ? 20 : 0, 20,
                candidate.YearsOfExperience > 0 ? Compatibility.Compatible : Compatibility.Unknown,
                $"{candidate.YearsOfExperience} years of experience are recorded."),
            Component("Required skills", (int)Math.Round(skillsRatio * 25), 25,
                missingSkills.Length == 0 ? Compatibility.Compatible : Compatibility.PartiallyCompatible,
                $"{matchedSkillCount} of {job.RequiredSkills.Count} listed skills match."),
            Component("Seniority", seniorityAligned ? 10 : 0, 10,
                !seniorityKnown ? Compatibility.Unknown :
                seniorityAligned ? Compatibility.Compatible : Compatibility.Incompatible,
                !seniorityKnown ? "The posting does not disclose seniority." :
                seniorityAligned ? "Candidate and job seniority align." : "Seniority does not align."),
            Component("Working-hour compatibility", hoursAligned ? 10 : 0, 10,
                !hoursKnown ? Compatibility.Unknown :
                hoursAligned ? Compatibility.Compatible : Compatibility.Unknown,
                !hoursKnown ? "The posting does not disclose working hours." :
                hoursAligned ? "The recorded timezone is compatible with the stated hours." :
                "Working-hour compatibility requires confirmation."),
            compensation
        };

        return new MatchResult(
            "v1",
            components.Sum(component => component.EarnedPoints),
            components,
            missingSkills);
    }

    private static MatchComponent AssessCompensation(
        CandidateSnapshot candidate,
        JobSnapshot job)
    {
        if (job.MinimumCompensation is null ||
            job.MaximumCompensation is null ||
            string.IsNullOrWhiteSpace(job.CompensationCurrency))
        {
            return Component(
                "Compensation compatibility",
                0,
                10,
                Compatibility.Unknown,
                "Compensation was not fully disclosed.");
        }

        if (candidate.MinimumCompensation is null ||
            string.IsNullOrWhiteSpace(candidate.CompensationCurrency) ||
            !string.Equals(
                candidate.CompensationCurrency,
                job.CompensationCurrency,
                StringComparison.OrdinalIgnoreCase))
        {
            return Component(
                "Compensation compatibility",
                0,
                10,
                Compatibility.Unknown,
                "Compensation cannot be compared in the same recorded currency.");
        }

        var compatible = job.MaximumCompensation >= candidate.MinimumCompensation;
        return Component(
            "Compensation compatibility",
            compatible ? 10 : 0,
            10,
            compatible ? Compatibility.Compatible : Compatibility.Incompatible,
            compatible
                ? "The disclosed range can meet the candidate minimum."
                : "The disclosed maximum is below the candidate minimum.");
    }

    private static MatchComponent Component(
        string name,
        int earned,
        int available,
        Compatibility compatibility,
        string explanation) =>
        new(name, earned, available, compatibility, explanation);

    private static bool SameMeaning(string left, string right)
    {
        var normalizedLeft = Normalize(left);
        var normalizedRight = Normalize(right);
        return normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal) ||
               normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal);
    }

    private static string Normalize(string value) =>
        string.Join(
            ' ',
            value.ToLowerInvariant()
                .Split([' ', '-', '/', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word is not "senior" and not "junior" and not "remote"));

    private static bool IsUtcCompatible(string timeZone, string workingHours) =>
        string.Equals(timeZone, "Africa/Accra", StringComparison.OrdinalIgnoreCase) &&
        workingHours.Contains("UTC", StringComparison.OrdinalIgnoreCase);
}
