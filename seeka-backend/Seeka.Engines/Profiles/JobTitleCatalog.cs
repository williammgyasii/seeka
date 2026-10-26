using System.Text.RegularExpressions;

namespace Seeka.Engines.Profiles;

/// <summary>
/// The job titles a candidate may target. Any name of an occupation finds it,
/// and it is always stored by its preferred name.
/// </summary>
public sealed partial class JobTitleCatalog
{
    private readonly Dictionary<string, string> preferredByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<(string Name, string[] Words, string Preferred)> names = [];

    public JobTitleCatalog(IEnumerable<Occupation> occupations)
    {
        foreach (var occupation in occupations)
        {
            Count++;
            foreach (var name in occupation.Alternatives.Prepend(occupation.Title))
            {
                var clean = Normalize(name);
                if (clean.Length == 0 || !preferredByName.TryAdd(clean, occupation.Title)) continue;
                names.Add((clean, Words(clean), occupation.Title));
            }
        }
    }

    public int Count { get; }

    /// <summary>The preferred name for any name of an occupation, or null when it is not on the list.</summary>
    public string? Preferred(string? title) =>
        title is not null && preferredByName.TryGetValue(Normalize(title), out var preferred) ? preferred : null;

    /// <summary>
    /// Occupations where every query word starts a word of one of their names. An exact
    /// name ranks first, then a name that starts with the query, then shorter titles.
    /// </summary>
    public IReadOnlyList<string> Search(string query, int limit)
    {
        var clean = Normalize(query);
        var queryWords = Words(clean);
        if (queryWords.Length == 0) return [];

        var best = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var (name, words, preferred) in names)
        {
            if (!queryWords.All(part => words.Any(word => word.StartsWith(part, StringComparison.OrdinalIgnoreCase))))
                continue;
            var rank = string.Equals(name, clean, StringComparison.OrdinalIgnoreCase) ? 0
                : name.StartsWith(clean, StringComparison.OrdinalIgnoreCase) ? 1
                : 2;
            if (!best.TryGetValue(preferred, out var current) || rank < current) best[preferred] = rank;
        }

        return best
            .OrderBy(pair => pair.Value)
            .ThenBy(pair => pair.Key.Length)
            .ThenBy(pair => pair.Key, StringComparer.Ordinal)
            .Take(limit)
            .Select(pair => pair.Key)
            .ToArray();
    }

    private static string Normalize(string value) => Spaces().Replace(value.Trim(), " ");

    private static string[] Words(string value) =>
        WordBreaks().Split(value).Where(word => word.Length > 0).ToArray();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex WordBreaks();
}
