using System.Text.RegularExpressions;

namespace Seeka.Engines.Eligibility;

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
