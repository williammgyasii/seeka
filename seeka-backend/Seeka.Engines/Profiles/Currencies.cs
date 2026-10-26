using System.Globalization;

namespace Seeka.Engines.Profiles;

/// <summary>Each country's own ISO 4217 currency, from the runtime's region data.</summary>
public static class Currencies
{
    private static readonly IReadOnlyDictionary<string, string> CurrencyByCountry = CultureInfo
        .GetCultures(CultureTypes.SpecificCultures)
        .Select(culture => Region(culture.Name))
        .OfType<RegionInfo>()
        .Where(region => region.TwoLetterISORegionName.Length == 2 &&
                         region.TwoLetterISORegionName.All(char.IsLetter) &&
                         region.ISOCurrencySymbol.Length == 3)
        .GroupBy(region => region.TwoLetterISORegionName, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key.ToUpperInvariant(), group => group.First().ISOCurrencySymbol);

    public static IEnumerable<string> Countries => CurrencyByCountry.Keys;

    /// <summary>The currency of a country by its ISO 3166 code, or null when unknown.</summary>
    public static string? ForCountry(string countryCode) =>
        CurrencyByCountry.GetValueOrDefault(countryCode.Trim().ToUpperInvariant());

    private static RegionInfo? Region(string cultureName)
    {
        try
        {
            return new RegionInfo(cultureName);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
