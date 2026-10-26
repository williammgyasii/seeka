using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace Seeka.Backend.Tests.Fakes;

internal static partial class TestBrowsers
{
    public static async Task<HttpClient> RegisteredAsync(SeekaApiFactory api, string email)
    {
        var browser = api.CreateBrowser();
        var response = await browser.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "Str0ng!Passw0rd",
            displayName = "Esi",
            phoneNumber = "+233241234567"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return browser;
    }

    public static async Task<HttpClient> VerifiedAsync(SeekaApiFactory api, string email = "esi@example.org")
    {
        var browser = await RegisteredAsync(api, email);
        var match = VerificationLink().Match(api.Email.Sent.Last(sent => sent.To == email).Content.Text);
        var verify = await browser.PostAsJsonAsync("/api/auth/verify-email", new
        {
            userId = Guid.Parse(match.Groups["userId"].Value),
            token = Uri.UnescapeDataString(match.Groups["token"].Value)
        });
        Assert.Equal(HttpStatusCode.NoContent, verify.StatusCode);
        return browser;
    }

    [GeneratedRegex(@"/verify-email/confirm\?userId=(?<userId>[0-9a-f-]+)&token=(?<token>[^\s]+)")]
    private static partial Regex VerificationLink();
}
