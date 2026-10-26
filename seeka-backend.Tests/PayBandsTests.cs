using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class PayBandsTests
{
    [Fact]
    public void Bands_cover_the_starter_currencies() =>
        Assert.Equal(
            ["CAD", "EUR", "GBP", "GHS", "KES", "NGN", "USD", "ZAR"],
            PayBands.ByCurrency.Keys.Order(StringComparer.Ordinal));

    [Fact]
    public void Ghana_cedi_bands_are_monthly_amounts_from_the_table() =>
        Assert.Equal(
            [
                new PayBand(0, 3_000), new PayBand(3_000, 6_000), new PayBand(6_000, 10_000),
                new PayBand(10_000, 20_000), new PayBand(20_000, 40_000), new PayBand(40_000, null)
            ],
            PayBands.ByCurrency["GHS"]);

    [Fact]
    public void Every_currency_has_six_bands_that_start_at_zero_touch_and_end_open()
    {
        foreach (var (currency, bands) in PayBands.ByCurrency)
        {
            Assert.True(bands.Count == 6, currency);
            Assert.Equal(0, bands[0].Minimum);
            Assert.Null(bands[^1].Maximum);
            for (var i = 1; i < bands.Count; i++)
                Assert.True(bands[i].Minimum == bands[i - 1].Maximum, $"{currency} band {i}");
        }
    }

    [Theory]
    [InlineData("GH", "GHS")]
    [InlineData("ng", "NGN")]
    [InlineData("KE", "KES")]
    [InlineData("DE", "EUR")]
    [InlineData("UG", "USD")]
    [InlineData("ZZ", "USD")]
    public void A_country_defaults_to_its_own_currency_or_usd(string countryCode, string expected) =>
        Assert.Equal(expected, PayBands.DefaultCurrency(countryCode));

    [Theory]
    [InlineData("GHS", 6_000, 10_000, true)]
    [InlineData("ghs", 40_000, null, true)]
    [InlineData("GHS", 5_000, 9_000, false)]
    [InlineData("GHS", 40_000, 80_000, false)]
    [InlineData("GHS", 300_000, 700_000, false)]
    [InlineData("JPY", 0, 3_000, false)]
    public void A_band_is_one_from_its_currency_list(string currency, int minimum, int? maximum, bool expected) =>
        Assert.Equal(expected, PayBands.IsBand(currency, minimum, maximum));
}
