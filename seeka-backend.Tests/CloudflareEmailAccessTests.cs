using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Seeka.Engines.Accounts;
using Seeka.Managers.Access.Email;
using Seeka.ResourceAccess.Accessors;

namespace Seeka.Backend.Tests;

public sealed class CloudflareEmailAccessTests
{
    private static readonly CloudflareEmailSettings Settings =
        new("acct-123", "token-abc", "verify@seekajobs.tech", "Seeka");

    private static readonly OutgoingEmail Email =
        new("esi@example.org", new EmailContent("Subject line", "Plain body", "<p>Html body</p>"));

    [Fact]
    public async Task Posts_to_the_account_send_endpoint_with_a_bearer_token()
    {
        var handler = new StubHandler(HttpStatusCode.OK);
        var access = Create(handler);

        Assert.True(await access.SendAsync(Email, CancellationToken.None));

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal(
            "https://api.cloudflare.com/client/v4/accounts/acct-123/email/sending/send",
            handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.Request.Headers.Authorization!.Scheme);
        Assert.Equal("token-abc", handler.Request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Sends_from_the_configured_address_with_text_and_html()
    {
        var handler = new StubHandler(HttpStatusCode.OK);

        await Create(handler).SendAsync(Email, CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Body!);
        var root = body.RootElement;
        Assert.Equal("esi@example.org", root.GetProperty("to").GetString());
        Assert.Equal("verify@seekajobs.tech", root.GetProperty("from").GetProperty("address").GetString());
        Assert.Equal("Seeka", root.GetProperty("from").GetProperty("name").GetString());
        Assert.Equal("Subject line", root.GetProperty("subject").GetString());
        Assert.Equal("Plain body", root.GetProperty("text").GetString());
        Assert.Equal("<p>Html body</p>", root.GetProperty("html").GetString());
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task Reports_failure_without_throwing(HttpStatusCode status) =>
        Assert.False(await Create(new StubHandler(status)).SendAsync(Email, CancellationToken.None));

    [Fact]
    public async Task Reports_failure_when_the_provider_is_unreachable() =>
        Assert.False(await Create(new StubHandler(null)).SendAsync(Email, CancellationToken.None));

    private static CloudflareEmailAccess Create(StubHandler handler) =>
        new(new HttpClient(handler), Settings, NullLogger<CloudflareEmailAccess>.Instance);

    private sealed class StubHandler(HttpStatusCode? status) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            Body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
            if (status is null) throw new HttpRequestException("unreachable");
            return new HttpResponseMessage(status.Value) { Content = new StringContent("{}") };
        }
    }
}
