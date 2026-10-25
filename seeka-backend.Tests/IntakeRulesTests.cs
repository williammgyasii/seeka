using Seeka.Engines.Intake;

namespace Seeka.Backend.Tests;

public sealed class IntakeRulesTests
{
    [Fact]
    public void Empty_optional_profile_url_is_normalized_to_null()
    {
        Assert.Null(IntakeRules.OptionalHttpUrl(""));
        Assert.Null(IntakeRules.OptionalHttpUrl("   "));
    }

    [Fact]
    public void Supplied_profile_url_must_use_http_or_https()
    {
        Assert.Equal(
            "https://example.com/profile",
            IntakeRules.OptionalHttpUrl(" https://example.com/profile "));
        Assert.Throws<ArgumentException>(() => IntakeRules.OptionalHttpUrl("file:///etc/passwd"));
    }

    [Fact]
    public void Canonical_url_drops_query_fragment_and_trailing_slash()
    {
        var source = IntakeRules.RequireHttpUrl("https://example.com/jobs/1/?utm=x#apply", "sourceUrl");

        Assert.Equal("https://example.com/jobs/1", IntakeRules.Canonicalize(source));
    }

    [Fact]
    public void Lists_are_trimmed_and_deduplicated_case_insensitively() =>
        Assert.Equal(["Excel", "Word"], IntakeRules.CleanList([" Excel ", "", "excel", "Word"]));
}
