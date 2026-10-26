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
    public async Task Asks_for_every_section_with_a_strict_schema_and_an_output_cap()
    {
        var handler = new StubHandler(Reply(Facts));

        await Create(handler).ExtractAsync("text", CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Requests.Single().Body);
        var root = body.RootElement;
        Assert.True(root.GetProperty("max_completion_tokens").GetInt32() > 0);
        var schema = root.GetProperty("response_format").GetProperty("json_schema").GetProperty("schema");
        var required = schema.GetProperty("required").EnumerateArray().Select(item => item.GetString()).ToArray();
        Assert.Superset(
            new HashSet<string?>(["headline", "experience", "education", "certifications", "languages", "links", "projects", "volunteering"]),
            new HashSet<string?>(required));
        var level = schema.GetProperty("properties").GetProperty("languages").GetProperty("items")
            .GetProperty("properties").GetProperty("level");
        Assert.Equal(
            ["Native", "Fluent", "Professional", "Conversational", "Basic", null],
            level.GetProperty("enum").EnumerateArray().Select(item => item.ValueKind == JsonValueKind.Null ? null : item.GetString()));
    }

    [Fact]
    public async Task Limits_seniority_to_the_fixed_levels()
    {
        var handler = new StubHandler(Reply(Facts));

        await Create(handler).ExtractAsync("text", CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Requests.Single().Body);
        var seniority = body.RootElement.GetProperty("response_format").GetProperty("json_schema")
            .GetProperty("schema").GetProperty("properties").GetProperty("seniority");
        Assert.Equal(
            ["Entry", "Junior", "Mid-level", "Senior", "Lead", "Manager", "Director", "Executive", null],
            seniority.GetProperty("enum").EnumerateArray().Select(item => item.ValueKind == JsonValueKind.Null ? null : item.GetString()));
    }

    [Fact]
    public async Task Asks_for_a_summary_without_pronouns()
    {
        var handler = new StubHandler(Reply(Facts));

        await Create(handler).ExtractAsync("text", CancellationToken.None);

        using var body = JsonDocument.Parse(handler.Requests.Single().Body);
        var summary = body.RootElement.GetProperty("response_format").GetProperty("json_schema")
            .GetProperty("schema").GetProperty("properties").GetProperty("experienceSummary")
            .GetProperty("description").GetString();
        Assert.Contains("no pronouns", summary);
        Assert.DoesNotContain("third person", summary);
    }

    [Fact]
    public async Task Maps_every_section()
    {
        const string full = """
            {"fullName":"Ama Mensah","headline":"HR operations lead","seniority":"Senior","yearsOfExperience":8,
             "experienceSummary":"Eight years in HR.","skills":["Recruitment"],"targetJobTitles":["HR Manager"],
             "experience":[{"company":"Acme","title":"HR Manager","location":"Accra","startMonth":"2021-06",
               "endMonth":null,"current":true,"highlights":["Ran payroll"]}],
             "education":[{"school":"University of Ghana","degree":"BSc","fieldOfStudy":"Psychology","startYear":2012,"endYear":2016}],
             "certifications":[{"name":"SHRM-CP","issuer":"SHRM","year":2020}],
             "languages":[{"language":"Twi","level":"Fluent"}],
             "links":[{"label":"LinkedIn","url":"https://linkedin.com/in/ama"}],
             "projects":[{"name":"Onboarding revamp","description":"Cut onboarding to a week.","url":null}],
             "volunteering":[{"organization":"Red Cross","role":"Coordinator","startMonth":"2019-01","endMonth":"2020-01",
               "current":false,"description":null}]}
            """;

        var facts = await Create(new StubHandler(Reply(full))).ExtractAsync("text", CancellationToken.None);

        Assert.Equal("HR operations lead", facts!.Headline);
        var details = facts.Details!;
        var job = Assert.Single(details.Experience);
        Assert.Equal(("Acme", "HR Manager", "Accra", "2021-06", true), (job.Company, job.Title, job.Location, job.StartMonth, job.Current));
        Assert.Equal(["Ran payroll"], job.Highlights);
        Assert.Equal(2016, Assert.Single(details.Education).EndYear);
        Assert.Equal("SHRM", Assert.Single(details.Certifications).Issuer);
        Assert.Equal("Fluent", Assert.Single(details.Languages).Level);
        Assert.Equal("https://linkedin.com/in/ama", Assert.Single(details.Links).Url);
        Assert.Null(Assert.Single(details.Projects).Url);
        Assert.Equal("Coordinator", Assert.Single(details.Volunteering).Role);
    }

    [Fact]
    public async Task A_reply_without_sections_has_empty_sections()
    {
        var facts = await Create(new StubHandler(Reply(Facts))).ExtractAsync("text", CancellationToken.None);

        Assert.Empty(facts!.Details!.Experience);
        Assert.Empty(facts.Details.Volunteering);
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
