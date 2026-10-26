using Seeka.Backend.Tests.Fakes;
using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class PreferencesPolicyTests
{
    private static readonly SearchPreferences Valid = new(
        ["human resources manager"], ["Full-time"], 6000, 10000, "GHS");

    private static IReadOnlyDictionary<string, string[]> Check(SearchPreferences preferences) =>
        PreferencesPolicy.Check(preferences, TestJobTitles.Catalog);

    [Fact]
    public void Complete_preferences_are_valid() => Assert.Empty(Check(Valid));

    [Fact]
    public void Types_and_pay_may_be_left_blank() =>
        Assert.Empty(Check(Valid with
        {
            EmploymentTypes = [], MinimumCompensation = null, MaximumCompensation = null, CompensationCurrency = null
        }));

    [Fact]
    public void At_least_one_title_is_needed() =>
        Assert.Equal(["Add at least one role you are looking for."], Check(Valid with { TargetJobTitles = [] })["targetJobTitles"]);

    [Fact]
    public void Titles_come_from_the_list() =>
        Assert.Equal(
            ["Choose a title from the list."],
            Check(Valid with { TargetJobTitles = ["human resources manager", "Chief Vibes Officer"] })["targetJobTitles[1]"]);

    [Fact]
    public void Employment_types_come_from_the_list() =>
        Assert.Equal(
            ["Choose a type from the list."],
            Check(Valid with { EmploymentTypes = ["Gig"] })["employmentTypes[0]"]);

    [Fact]
    public void The_top_band_has_no_maximum() =>
        Assert.Empty(Check(Valid with { MinimumCompensation = 40000, MaximumCompensation = null }));

    [Theory]
    [InlineData(5000, 9000)]
    [InlineData(6000, null)]
    [InlineData(null, 10000)]
    [InlineData(300000, 700000)]
    public void Pay_must_be_one_of_the_currency_bands(int? minimum, int? maximum) =>
        Assert.Equal(
            ["Choose a pay band from the list."],
            Check(Valid with { MinimumCompensation = minimum, MaximumCompensation = maximum })["minimumCompensation"]);

    [Fact]
    public void A_band_needs_a_currency() =>
        Assert.Equal(
            ["Choose a currency for the pay."],
            Check(Valid with { CompensationCurrency = null })["compensationCurrency"]);

    [Fact]
    public void The_currency_must_have_bands() =>
        Assert.Equal(
            ["Choose a currency from the list."],
            Check(Valid with { CompensationCurrency = "JPY" })["compensationCurrency"]);

    [Fact]
    public void Tidy_stores_titles_and_types_by_their_list_names()
    {
        var tidy = PreferencesPolicy.Tidy(
            Valid with
            {
                TargetJobTitles = [" HR Manager ", "human resources manager", "Chief Vibes Officer"],
                EmploymentTypes = ["full-time", "Contract"],
                CompensationCurrency = " usd "
            },
            TestJobTitles.Catalog);

        Assert.Equal(["human resources manager", "Chief Vibes Officer"], tidy.TargetJobTitles);
        Assert.Equal(["Full-time", "Contract"], tidy.EmploymentTypes);
        Assert.Equal("USD", tidy.CompensationCurrency);
    }
}
