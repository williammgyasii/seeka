using System.Text.RegularExpressions;

namespace Seeka.Engines.Profiles;

/// <summary>The fixed seniority levels shared by the résumé reader, the About you form, and the server.</summary>
public static partial class SeniorityLevel
{
    public static readonly IReadOnlyList<string> All =
        ["Entry", "Junior", "Mid-level", "Senior", "Lead", "Manager", "Director", "Executive"];

    /// <summary>Highest level first, so "Senior Manager" reads as Manager.</summary>
    private static readonly (string Level, Regex Words)[] LegacyWords =
    [
        ("Executive", ExecutiveWords()),
        ("Director", DirectorWords()),
        ("Manager", ManagerWords()),
        ("Lead", LeadWords()),
        ("Senior", SeniorWords()),
        ("Mid-level", MidWords()),
        ("Junior", JuniorWords()),
        ("Entry", EntryWords())
    ];

    /// <summary>The level in its listed spelling, or null when the value is not on the list.</summary>
    public static string? Exact(string? value) =>
        All.FirstOrDefault(level => string.Equals(level, value?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Reads free text saved before the fixed list as the highest level it names,
    /// or null when it names none.
    /// </summary>
    public static string? FromLegacy(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Exact(value) ?? LegacyWords.FirstOrDefault(pair => pair.Words.IsMatch(value)).Level;
    }

    [GeneratedRegex(@"\b(executive|chief|vp|vice president|c-level|ceo|cfo|coo|cto)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ExecutiveWords();

    [GeneratedRegex(@"\b(director|head of)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DirectorWords();

    [GeneratedRegex(@"\bmanager\b", RegexOptions.IgnoreCase)]
    private static partial Regex ManagerWords();

    [GeneratedRegex(@"\b(lead|principal)\b", RegexOptions.IgnoreCase)]
    private static partial Regex LeadWords();

    [GeneratedRegex(@"\b(senior|sr)\b", RegexOptions.IgnoreCase)]
    private static partial Regex SeniorWords();

    [GeneratedRegex(@"\bmid\b", RegexOptions.IgnoreCase)]
    private static partial Regex MidWords();

    [GeneratedRegex(@"\b(junior|jr)\b", RegexOptions.IgnoreCase)]
    private static partial Regex JuniorWords();

    [GeneratedRegex(@"\b(entry|intern|graduate|trainee)\b", RegexOptions.IgnoreCase)]
    private static partial Regex EntryWords();
}
