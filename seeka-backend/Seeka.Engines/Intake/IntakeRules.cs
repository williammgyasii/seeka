namespace Seeka.Engines.Intake;

public static class IntakeRules
{
    public static Uri RequireHttpUrl(string value, string parameterName)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || !IsHttp(uri))
            throw new ArgumentException("Only absolute HTTP and HTTPS URLs are allowed.", parameterName);
        return uri;
    }

    public static string Canonicalize(Uri uri)
    {
        var builder = new UriBuilder(uri) { Fragment = "", Query = "" };
        return builder.Uri.ToString().TrimEnd('/');
    }

    public static string? OptionalHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri) || !IsHttp(uri))
            throw new ArgumentException("Only absolute HTTP and HTTPS URLs are allowed.");
        return uri.ToString().TrimEnd('/');
    }

    public static string? OptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string[] CleanList(IEnumerable<string> values) =>
        values.Select(value => value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static string? Currency(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    private static bool IsHttp(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
}
