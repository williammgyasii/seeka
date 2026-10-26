using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Seeka.ResourceAccess.Accessors;

namespace Seeka.Backend.Tests;

public sealed class OpenAiResumeExtractionAccessTests
{
    private const string Facts = """
        {"fullName":"Ama Mensah","seniority":"Senior","yearsOfExperience":8,
         "experienceSummary":"Eight years in HR.","skills":["Recruitment"],"targetJobTitles":["HR Manager"]}
        """;

    [Fact]
    public async Task Sends_the_text_with_a_strict_schema_to_the_configured_model()
    {
        var handler = new StubHandler(Reply(Facts));

        await Create(handler).ExtractAsync("Ama Mensah, HR Manager", CancellationToken.None);

        var request = handler.Requests.Single();
        Assert.Equal("https://api.openai.com/v1/chat/completions", request.Uri);
        Assert.Equal("Bearer sk-test", request.Authorization);
        using var body = JsonDocument.Parse(request.Body);
        var root = body.RootElement;
        Assert.Equal("gpt-test", root.GetProperty("model").GetString());
        var format = root.GetProperty("response_format");
        Assert.Equal("json_schema", format.GetProperty("type").GetString());
        Assert.True(format.GetProperty("json_schema").GetProperty("strict").GetBoolean());
        var user = root.GetProperty("messages").EnumerateArray().Last();
        Assert.Equal("user", user.GetProperty("role").GetString());
        Assert.Contains("Ama Mensah, HR Manager", user.GetProperty("content").GetString());
    }

    [Fact]
    public async Task Maps_the_reply_to_facts()
    {
        var facts = await Create(new StubHandler(Reply(Facts))).ExtractAsync("text", CancellationToken.None);

        Assert.Equal("Ama Mensah", facts!.FullName);
        Assert.Equal(8, facts.YearsOfExperience);
        Assert.Equal(["Recruitment"], facts.Skills);
        Assert.Equal(["HR Manager"], facts.TargetJobTitles);
    }

    [Fact]
    public async Task Missing_facts_stay_null()
    {
        const string sparse = """
            {"fullName":null,"seniority":null,"yearsOfExperience":null,
             "experienceSummary":null,"skills":[],"targetJobTitles":[]}
            """;

        var facts = await Create(new StubHandler(Reply(sparse))).ExtractAsync("text", CancellationToken.None);

        Assert.Null(facts!.FullName);
        Assert.Null(facts.YearsOfExperience);
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task Retries_once_after_a_transient_failure(HttpStatusCode status)
    {
        var handler = new StubHandler(Status(status), Reply(Facts));

        var facts = await Create(handler).ExtractAsync("text", CancellationToken.None);

        Assert.Equal("Ama Mensah", facts!.FullName);
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Gives_up_after_the_retry()
    {
        var handler = new StubHandler(
            Status(HttpStatusCode.InternalServerError),
            Status(HttpStatusCode.InternalServerError),
            Reply(Facts));

        Assert.Null(await Create(handler).ExtractAsync("text", CancellationToken.None));
        Assert.Equal(2, handler.Requests.Count);
    }

    [Fact]
    public async Task Does_not_retry_a_client_error()
    {
        var handler = new StubHandler(Status(HttpStatusCode.Unauthorized), Reply(Facts));

        Assert.Null(await Create(handler).ExtractAsync("text", CancellationToken.None));
        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Refusal_or_malformed_reply_is_a_failure()
    {
        Assert.Null(await Create(new StubHandler(Reply("not json"))).ExtractAsync("text", CancellationToken.None));
        Assert.Null(await Create(new StubHandler(Refusal())).ExtractAsync("text", CancellationToken.None));
    }

    [Fact]
    public async Task Unreachable_service_is_a_failure() =>
        Assert.Null(await Create(new StubHandler(null, null)).ExtractAsync("text", CancellationToken.None));

    [Fact]
    public async Task Long_text_is_shortened_before_sending()
    {
        var handler = new StubHandler(Reply(Facts));

        await Create(handler).ExtractAsync(new string('a', 100_000), CancellationToken.None);

        Assert.True(handler.Requests.Single().Body.Length < 40_000);
    }

    private static OpenAiResumeExtractionAccess Create(StubHandler handler) =>
        new(new HttpClient(handler), new OpenAiSettings("sk-test", "gpt-test"),
            NullLogger<OpenAiResumeExtractionAccess>.Instance);

    private static Func<HttpResponseMessage> Reply(string content) =>
        () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { role = "assistant", content, refusal = (string?)null } } }
            }))
        };

    private static Func<HttpResponseMessage> Refusal() =>
        () => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                choices = new[] { new { message = new { role = "assistant", content = (string?)null, refusal = "No." } } }
            }))
        };

    private static Func<HttpResponseMessage> Status(HttpStatusCode status) =>
        () => new HttpResponseMessage(status) { Content = new StringContent("{}") };

    private sealed record SentRequest(string Uri, string? Authorization, string Body);

    private sealed class StubHandler(params Func<HttpResponseMessage>?[] responses) : HttpMessageHandler
    {
        private int next;

        public List<SentRequest> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(new SentRequest(
                request.RequestUri!.ToString(),
                request.Headers.Authorization?.ToString(),
                request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellationToken)));
            var response = responses[Math.Min(next++, responses.Length - 1)];
            return response is null ? throw new HttpRequestException("unreachable") : response();
        }
    }
}
