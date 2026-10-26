using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class CurrenciesTests
{
    [Theory]
    [InlineData("GH", "GHS")]
    [InlineData("US", "USD")]
    [InlineData("PT", "EUR")]
    [InlineData("ng", "NGN")]
    [InlineData("ZZ", null)]
    public void A_country_has_its_own_currency(string countryCode, string? expected) =>
        Assert.Equal(expected, Currencies.ForCountry(countryCode));
}
