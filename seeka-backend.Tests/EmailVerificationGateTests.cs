using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Seeka.Backend.Tests.Fakes;

namespace Seeka.Backend.Tests;

public sealed partial class EmailVerificationGateTests : IDisposable
{
    private readonly SeekaApiFactory api = new();

    public void Dispose() => api.Dispose();

    [Fact]
    public async Task Unverified_person_is_signed_in_but_locked_out_of_the_workspace()
    {
        var browser = await RegisterAsync("esi@example.org");

        var session = await browser.GetFromJsonAsync<JsonElement>("/api/auth/session");
        var pipeline = await browser.GetAsync("/api/pipeline");
        var candidate = await browser.GetAsync("/api/candidate/profile");

        Assert.False(session.GetProperty("emailVerified").GetBoolean());
        Assert.Equal(HttpStatusCode.Forbidden, pipeline.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, candidate.StatusCode);
    }

    [Fact]
    public async Task Signed_out_caller_gets_401_not_403()
    {
        var response = await api.CreateBrowser().GetAsync("/api/pipeline");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Link_opened_on_another_device_unlocks_the_workspace()
    {
        var laptop = await RegisterAsync("esi@example.org");
        var (userId, token) = LinkFrom(api.Email.Sent.Single().Content.Text);

        var phone = api.CreateBrowser();
        var verify = await phone.PostAsJsonAsync("/api/auth/verify-email", new { userId, token });

        Assert.Equal(HttpStatusCode.NoContent, verify.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await laptop.GetAsync("/api/pipeline")).StatusCode);
        var session = await laptop.GetFromJsonAsync<JsonElement>("/api/auth/session");
        Assert.True(session.GetProperty("emailVerified").GetBoolean());
    }

    [Fact]
    public async Task Altered_link_is_rejected()
    {
        await RegisterAsync("esi@example.org");
        var (userId, token) = LinkFrom(api.Email.Sent.Single().Content.Text);

        var verify = await api.CreateBrowser()
            .PostAsJsonAsync("/api/auth/verify-email", new { userId, token = token + "x" });

        Assert.Equal(HttpStatusCode.BadRequest, verify.StatusCode);
    }

    [Fact]
    public async Task Unverified_person_can_resend_the_link()
    {
        var browser = await RegisterAsync("esi@example.org");

        var resend = await browser.PostAsync("/api/auth/verification-email", null);

        Assert.Equal(HttpStatusCode.Accepted, resend.StatusCode);
        Assert.Equal(2, api.Email.Sent.Count);
    }

    [Fact]
    public async Task Verified_person_cannot_resend()
    {
        var browser = await RegisterAsync("esi@example.org");
        var (userId, token) = LinkFrom(api.Email.Sent.Single().Content.Text);
        await browser.PostAsJsonAsync("/api/auth/verify-email", new { userId, token });

        var resend = await browser.PostAsync("/api/auth/verification-email", null);

        Assert.Equal(HttpStatusCode.Conflict, resend.StatusCode);
    }

    [Fact]
    public async Task Correcting_the_address_keeps_the_person_signed_in_and_mails_the_new_address()
    {
        var browser = await RegisterAsync("esi@exmaple.org");

        var change = await browser.PutAsJsonAsync("/api/auth/email", new { email = "esi@example.org" });

        Assert.Equal(HttpStatusCode.Accepted, change.StatusCode);
        var session = await browser.GetFromJsonAsync<JsonElement>("/api/auth/session");
        Assert.Equal("esi@example.org", session.GetProperty("email").GetString());
        Assert.Equal("esi@example.org", api.Email.Sent.Last().To);
    }

    [Fact]
    public async Task Correcting_to_an_address_in_use_returns_an_email_field_error()
    {
        await RegisterAsync("taken@example.org");
        var browser = await RegisterAsync("esi@exmaple.org");

        var change = await browser.PutAsJsonAsync("/api/auth/email", new { email = "taken@example.org" });

        Assert.Equal(HttpStatusCode.BadRequest, change.StatusCode);
        var problem = await change.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("email", out _));
    }

    private async Task<HttpClient> RegisterAsync(string email)
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

    private static (Guid UserId, string Token) LinkFrom(string text)
    {
        var match = VerificationLink().Match(text);
        Assert.True(match.Success, text);
        return (Guid.Parse(match.Groups["userId"].Value), Uri.UnescapeDataString(match.Groups["token"].Value));
    }

    [GeneratedRegex(@"/verify-email/confirm\?userId=(?<userId>[0-9a-f-]+)&token=(?<token>[^\s]+)")]
    private static partial Regex VerificationLink();
}
