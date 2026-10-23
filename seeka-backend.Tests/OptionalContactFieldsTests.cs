using Seeka.Application;

namespace Seeka.Backend.Tests;

public sealed class OptionalContactFieldsTests
{
    [Fact]
    public void Empty_optional_profile_url_is_normalized_to_null()
    {
        Assert.Null(OptionalContactFields.NormalizeHttpUrl(""));
        Assert.Null(OptionalContactFields.NormalizeHttpUrl("   "));
    }

    [Fact]
    public void Supplied_profile_url_must_use_http_or_https()
    {
        Assert.Equal(
            "https://example.com/profile",
            OptionalContactFields.NormalizeHttpUrl(" https://example.com/profile "));
        Assert.Throws<ArgumentException>(() =>
            OptionalContactFields.NormalizeHttpUrl("file:///etc/passwd"));
    }
}
