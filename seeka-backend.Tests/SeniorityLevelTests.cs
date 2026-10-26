using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class SeniorityLevelTests
{
    [Fact]
    public void The_levels_run_from_entry_to_executive() =>
        Assert.Equal(
            ["Entry", "Junior", "Mid-level", "Senior", "Lead", "Manager", "Director", "Executive"],
            SeniorityLevel.All);

    [Theory]
    [InlineData("senior", "Senior")]
    [InlineData(" MID-LEVEL ", "Mid-level")]
    [InlineData("Senior HR Business Partner", null)]
    [InlineData(null, null)]
    public void Exact_accepts_only_a_level_from_the_list(string? value, string? expected) =>
        Assert.Equal(expected, SeniorityLevel.Exact(value));

    [Theory]
    [InlineData("Senior HR Business Partner", "Senior")]
    [InlineData("Senior Manager, People", "Manager")]
    [InlineData("HR Director", "Director")]
    [InlineData("Head of People", "Director")]
    [InlineData("VP Operations", "Executive")]
    [InlineData("Chief People Officer", "Executive")]
    [InlineData("Team Lead", "Lead")]
    [InlineData("Jr. Analyst", "Junior")]
    [InlineData("Graduate trainee", "Entry")]
    [InlineData("Mid-level", "Mid-level")]
    [InlineData("People Operations Specialist", null)]
    [InlineData("", null)]
    public void FromLegacy_reads_the_highest_level_named_in_old_free_text(string? value, string? expected) =>
        Assert.Equal(expected, SeniorityLevel.FromLegacy(value));
}
