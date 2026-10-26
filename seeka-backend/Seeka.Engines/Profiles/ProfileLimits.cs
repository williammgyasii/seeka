using System.Text.RegularExpressions;

namespace Seeka.Engines.Profiles;

/// <summary>
/// Limits shared by résumé cleanup and step validation, so anything the draft
/// pre-fills can be saved unchanged.
/// </summary>
public static partial class ProfileLimits
{
    public const int MaxExperience = 15;
    public const int MaxHighlights = 8;
    public const int MaxHighlightLength = 300;
    public const int MaxEducation = 10;
    public const int MaxCertifications = 20;
    public const int MaxLanguages = 10;
    public const int MaxLinks = 5;
    public const int MaxProjects = 10;
    public const int MaxVolunteering = 10;
    public const int MaxDescriptionLength = 600;
    public const int MaxTextLength = 160;
    public const int MaxHeadlineLength = 120;
    public const int MinYear = 1950;
    public const int MaxYear = 2100;

    public static readonly string[] LanguageLevels = ["Native", "Fluent", "Professional", "Conversational", "Basic"];

    /// <summary>Months since year zero, or null when the value is not a <c>YYYY-MM</c> month in range.</summary>
    public static int? MonthIndex(string? value)
    {
        if (value is null || !Month().IsMatch(value)) return null;
        var year = int.Parse(value[..4]);
        var month = int.Parse(value[5..]);
        return IsYear(year) ? year * 12 + month - 1 : null;
    }

    public static bool IsYear(int? year) => year is >= MinYear and <= MaxYear;

    public static bool IsHttps(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;

    public static string? Level(string? value) =>
        LanguageLevels.FirstOrDefault(level => string.Equals(level, value?.Trim(), StringComparison.OrdinalIgnoreCase));

    [GeneratedRegex(@"^\d{4}-(0[1-9]|1[0-2])$")]
    private static partial Regex Month();
}
