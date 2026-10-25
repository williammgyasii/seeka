using Seeka.Engines.Accounts;

namespace Seeka.Backend.Tests;

public sealed class PhoneNumberPolicyTests
{
    [Theory]
    [InlineData("+351912345678", "+351912345678")]
    [InlineData("+233241234567", "+233241234567")]
    [InlineData("+351 912 345 678", "+351912345678")]
    [InlineData(" +44 7400 123456 ", "+447400123456")]
    public void Valid_numbers_normalise_to_E164(string input, string expected) =>
        Assert.Equal(expected, PhoneNumberPolicy.ToE164(input));

    [Theory]
    [InlineData("+15550000000")]
    [InlineData("12345")]
    [InlineData("0912345678")]
    [InlineData("")]
    [InlineData("not a phone")]
    public void Invalid_numbers_are_rejected(string input) =>
        Assert.Null(PhoneNumberPolicy.ToE164(input));
}
