using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Seeka.Engines.Profiles;
using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class OpenAiResumeExtractionAccess(
    HttpClient http,
    OpenAiSettings settings,
    ILogger<OpenAiResumeExtractionAccess> logger) : IResumeExtractionAccess
{
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";

    /// <summary>Bounds the cost of one call; a résumé rarely exceeds a few thousand words.</summary>
    private const int MaxTextLength = 30_000;

    /// <summary>Room for the longest résumé the schema limits allow, and no more.</summary>
    private const int MaxOutputTokens = 8_000;

    private const string Instructions = """
        You extract facts from a résumé. Report only what the résumé states.
        Use null, or an empty list, for anything it does not state. Do not guess,
        infer, rate, or recommend. Seniority is the level of the most recent role,
        chosen from the listed levels; use null when no level fits. The summary
        uses no pronouns: begin with what the person did. Target job titles are the roles
        the résumé says the person wants, or else their most recent titles.
        Write months as YYYY-MM; use null when the résumé gives no month. Copy
        highlights as the résumé words them. Mark a role current only when the
        résumé says it is ongoing.
        """;

    private static readonly object Schema = Object(new()
    {
        ["fullName"] = Nullable("string"),
        ["headline"] = Nullable("string", "The résumé's own one-line title or tagline."),
        ["seniority"] = new
        {
            type = new[] { "string", "null" },
            @enum = SeniorityLevel.All.Cast<string?>().Append(null).ToArray()
        },
        ["yearsOfExperience"] = Nullable("integer"),
        ["experienceSummary"] = Nullable(
            "string",
            "Two or three sentences drawn from the résumé, with no pronouns, such as \"Runs HR for growing teams.\""),
        ["skills"] = Strings(),
        ["targetJobTitles"] = Strings(),
        ["experience"] = ArrayOf(new()
        {
            ["company"] = Nullable("string"),
            ["title"] = Nullable("string"),
            ["location"] = Nullable("string"),
            ["startMonth"] = Nullable("string", "YYYY-MM"),
            ["endMonth"] = Nullable("string", "YYYY-MM"),
            ["current"] = new { type = "boolean" },
            ["highlights"] = Strings()
        }),
        ["education"] = ArrayOf(new()
        {
            ["school"] = Nullable("string"),
            ["degree"] = Nullable("string"),
            ["fieldOfStudy"] = Nullable("string"),
            ["startYear"] = Nullable("integer"),
            ["endYear"] = Nullable("integer")
        }),
        ["certifications"] = ArrayOf(new()
        {
            ["name"] = Nullable("string"),
            ["issuer"] = Nullable("string"),
            ["year"] = Nullable("integer")
        }),
        ["languages"] = ArrayOf(new()
        {
            ["language"] = Nullable("string"),
            ["level"] = new
            {
                type = new[] { "string", "null" },
                @enum = ProfileLimits.LanguageLevels.Cast<string?>().Append(null).ToArray()
            }
        }),
        ["links"] = ArrayOf(new()
        {
            ["label"] = Nullable("string"),
            ["url"] = Nullable("string")
        }),
        ["projects"] = ArrayOf(new()
        {
            ["name"] = Nullable("string"),
            ["description"] = Nullable("string"),
            ["url"] = Nullable("string")
        }),
        ["volunteering"] = ArrayOf(new()
        {
            ["organization"] = Nullable("string"),
            ["role"] = Nullable("string"),
            ["startMonth"] = Nullable("string", "YYYY-MM"),
            ["endMonth"] = Nullable("string", "YYYY-MM"),
            ["current"] = new { type = "boolean" },
            ["description"] = Nullable("string")
        })
    });

    /// <summary>Strict mode needs every property listed as required, so absence is expressed as null.</summary>
    private static object Object(Dictionary<string, object> properties) => new
    {
        type = "object",
        additionalProperties = false,
        required = properties.Keys.ToArray(),
        properties
    };

    private static object ArrayOf(Dictionary<string, object> itemProperties) =>
        new { type = "array", items = Object(itemProperties) };

    private static object Strings() => new { type = "array", items = new { type = "string" } };

    private static object Nullable(string type, string? description = null) =>
        description is null
            ? new { type = new[] { type, "null" } }
            : new { type = new[] { type, "null" }, description };

    public async Task<ResumeFacts?> ExtractAsync(string resumeText, CancellationToken cancellationToken)
    {
        var text = resumeText.Length > MaxTextLength ? resumeText[..MaxTextLength] : resumeText;
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                using var response = await http.SendAsync(Request(text), cancellationToken);
                if (response.IsSuccessStatusCode)
                    return Parse(await response.Content.ReadAsStringAsync(cancellationToken));
                var retry = IsTransient(response.StatusCode) && attempt == 1;
                logger.LogWarning(
                    "OpenAI rejected a résumé extraction with status {Status} (attempt {Attempt}).",
                    (int)response.StatusCode,
                    attempt);
                if (!retry) return null;
            }
            catch (Exception exception) when (
                exception is HttpRequestException ||
                exception is TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "OpenAI was unreachable (attempt {Attempt}).", attempt);
                if (attempt == 2) return null;
            }
        }

        return null;
    }

    private HttpRequestMessage Request(string text)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = JsonContent.Create(new
            {
                model = settings.Model,
                max_completion_tokens = MaxOutputTokens,
                messages = new object[]
                {
                    new { role = "system", content = Instructions },
                    new { role = "user", content = text }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new { name = "resume_facts", strict = true, schema = Schema }
                }
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        return request;
    }

    private ResumeFacts? Parse(string body)
    {
        try
        {
            using var reply = JsonDocument.Parse(body);
            var message = reply.RootElement.GetProperty("choices")[0].GetProperty("message");
            if (message.GetProperty("content").GetString() is not { } content) return null;

            using var facts = JsonDocument.Parse(content);
            var root = facts.RootElement;
            return new ResumeFacts(
                Text(root, "fullName"),
                Text(root, "seniority"),
                root.GetProperty("yearsOfExperience") is { ValueKind: JsonValueKind.Number } years
                    && years.TryGetInt32(out var value) ? value : null,
                Text(root, "experienceSummary"),
                List(root, "skills"),
                List(root, "targetJobTitles"))
            {
                Headline = Optional(root, "headline"),
                Details = new ProfileDetails(
                    Items(root, "experience", item => new WorkExperience(
                        Optional(item, "company") ?? "",
                        Optional(item, "title") ?? "",
                        Optional(item, "location"),
                        Optional(item, "startMonth"),
                        Optional(item, "endMonth"),
                        item.TryGetProperty("current", out var current) && current.ValueKind == JsonValueKind.True,
                        OptionalList(item, "highlights"))),
                    Items(root, "education", item => new Education(
                        Optional(item, "school") ?? "",
                        Optional(item, "degree"),
                        Optional(item, "fieldOfStudy"),
                        Number(item, "startYear"),
                        Number(item, "endYear"))),
                    Items(root, "certifications", item => new Certification(
                        Optional(item, "name") ?? "",
                        Optional(item, "issuer"),
                        Number(item, "year"))),
                    Items(root, "languages", item => new LanguageSkill(
                        Optional(item, "language") ?? "",
                        Optional(item, "level"))),
                    Items(root, "links", item => new ProfileLink(
                        Optional(item, "label"),
                        Optional(item, "url") ?? "")),
                    Items(root, "projects", item => new Project(
                        Optional(item, "name") ?? "",
                        Optional(item, "description"),
                        Optional(item, "url"))),
                    Items(root, "volunteering", item => new Volunteering(
                        Optional(item, "organization") ?? "",
                        Optional(item, "role"),
                        Optional(item, "startMonth"),
                        Optional(item, "endMonth"),
                        item.TryGetProperty("current", out var ongoing) && ongoing.ValueKind == JsonValueKind.True,
                        Optional(item, "description"))))
            };
        }
        catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            logger.LogWarning(exception, "OpenAI returned a résumé extraction that did not match the schema.");
            return null;
        }
    }

    private static string? Text(JsonElement root, string name) =>
        root.GetProperty(name) is { ValueKind: JsonValueKind.String } value ? value.GetString() : null;

    private static string[] List(JsonElement root, string name) =>
        root.GetProperty(name).EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray();

    private static string? Optional(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static int? Number(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number)
            ? number
            : null;

    private static string[] OptionalList(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToArray()
            : [];

    private static T[] Items<T>(JsonElement root, string name, Func<JsonElement, T> read) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray().Where(item => item.ValueKind == JsonValueKind.Object).Select(read).ToArray()
            : [];

    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests || (int)status >= 500;
}
