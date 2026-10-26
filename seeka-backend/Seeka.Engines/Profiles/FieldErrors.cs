namespace Seeka.Engines.Profiles;

/// <summary>Collects messages by field path, such as <c>experience[1].endMonth</c>.</summary>
internal sealed class FieldErrors
{
    private readonly Dictionary<string, List<string>> errors = [];

    public void Add(string field, string message)
    {
        if (!errors.TryGetValue(field, out var messages)) errors[field] = messages = [];
        messages.Add(message);
    }

    public void Require(string field, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value)) Add(field, message);
    }

    public void MaxLength(string field, string? value, int max)
    {
        if (value?.Trim().Length > max) Add(field, $"Keep this under {max + 1} characters.");
    }

    public void MaxCount<T>(string field, IReadOnlyCollection<T> items, int max)
    {
        if (items.Count > max) Add(field, $"Add at most {max}.");
    }

    public void Year(string field, int? year)
    {
        if (year is not null && !ProfileLimits.IsYear(year))
            Add(field, $"Enter a year between {ProfileLimits.MinYear} and {ProfileLimits.MaxYear}.");
    }

    public void Https(string field, string? url, bool required)
    {
        if (!required && string.IsNullOrWhiteSpace(url)) return;
        if (!ProfileLimits.IsHttps(url?.Trim())) Add(field, "Enter a full link starting with https://.");
    }

    public void Period(string path, string? startMonth, string? endMonth, bool current)
    {
        var start = Month($"{path}.startMonth", startMonth);
        var end = Month($"{path}.endMonth", endMonth);
        if (current && endMonth is not null)
            Add($"{path}.endMonth", "A current role has no end month.");
        else if (start is not null && end is not null && end < start)
            Add($"{path}.endMonth", "The end month can't be before the start month.");
    }

    private int? Month(string field, string? value)
    {
        if (value is null) return null;
        var index = ProfileLimits.MonthIndex(value);
        if (index is null) Add(field, "Use year and month, like 2021-06.");
        return index;
    }

    public IReadOnlyDictionary<string, string[]> ToDictionary() =>
        errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray());
}
