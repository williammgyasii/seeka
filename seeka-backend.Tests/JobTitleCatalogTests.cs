using Seeka.Backend.Tests.Fakes;

namespace Seeka.Backend.Tests;

public sealed class JobTitleCatalogTests
{
    [Fact]
    public void Search_finds_an_occupation_by_its_alternative_name() =>
        Assert.Equal("human resources manager", TestJobTitles.Catalog.Search("hr manager", 10)[0]);

    [Fact]
    public void Search_matches_the_start_of_each_word_in_any_case() =>
        Assert.Equal(["human resources manager", "human resources officer"], TestJobTitles.Catalog.Search("HUMAN res", 10));

    [Fact]
    public void Search_returns_each_occupation_once_by_its_preferred_name() =>
        Assert.Equal(["software developer"], TestJobTitles.Catalog.Search("soft", 10));

    [Fact]
    public void Search_stops_at_the_limit() =>
        Assert.Single(TestJobTitles.Catalog.Search("human", 1));

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("astronaut")]
    public void Search_with_no_match_is_empty(string query) =>
        Assert.Empty(TestJobTitles.Catalog.Search(query, 10));

    [Theory]
    [InlineData("human resources manager", "human resources manager")]
    [InlineData(" HR Manager ", "human resources manager")]
    [InlineData("People Partner", "human resources officer")]
    [InlineData("Chief Vibes Officer", null)]
    public void Preferred_turns_any_label_into_the_preferred_name(string title, string? expected) =>
        Assert.Equal(expected, TestJobTitles.Catalog.Preferred(title));
}
