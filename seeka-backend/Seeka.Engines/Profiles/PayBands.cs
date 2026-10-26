namespace Seeka.Engines.Profiles;

/// <summary>The monthly pay bands Seeka offers, by ISO 4217 currency.</summary>
public static class PayBands
{
    public const string FallbackCurrency = "USD";

    public static readonly IReadOnlyDictionary<string, IReadOnlyList<PayBand>> ByCurrency =
        new Dictionary<string, IReadOnlyList<PayBand>>(StringComparer.OrdinalIgnoreCase)
        {
            ["GHS"] = Bands(3_000, 6_000, 10_000, 20_000, 40_000),
            ["NGN"] = Bands(300_000, 700_000, 1_500_000, 3_000_000, 6_000_000),
            ["KES"] = Bands(50_000, 100_000, 200_000, 400_000, 800_000),
            ["ZAR"] = Bands(15_000, 30_000, 50_000, 80_000, 120_000),
            ["USD"] = Bands(1_000, 2_500, 5_000, 8_000, 12_000),
            ["GBP"] = Bands(2_000, 3_000, 4_500, 6_500, 9_000),
            ["EUR"] = Bands(2_000, 3_500, 5_000, 7_000, 10_000),
            ["CAD"] = Bands(3_000, 5_000, 7_500, 10_000, 14_000)
        };

    /// <summary>Every known country's default pay currency, keyed by ISO 3166 code.</summary>
    public static readonly IReadOnlyDictionary<string, string> DefaultByCountry =
        Currencies.Countries.ToDictionary(country => country, DefaultCurrency);

    /// <summary>The country's own currency when it has bands, otherwise USD.</summary>
    public static string DefaultCurrency(string countryCode) =>
        Currencies.ForCountry(countryCode) is { } own && ByCurrency.ContainsKey(own) ? own : FallbackCurrency;

    public static bool HasBands(string? currency) => currency is not null && ByCurrency.ContainsKey(currency.Trim());

    public static bool IsBand(string? currency, decimal? minimum, decimal? maximum) =>
        HasBands(currency) &&
        minimum is { } low &&
        ByCurrency[currency!.Trim()].Contains(new PayBand(low, maximum));

    private static PayBand[] Bands(params decimal[] edges) =>
        edges.Prepend(0).Select((minimum, i) => new PayBand(minimum, i < edges.Length ? edges[i] : null)).ToArray();
}
