using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
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

    private const string Instructions = """
        You extract facts from a résumé. Report only what the résumé states.
        Use null, or an empty list, for anything it does not state. Do not guess,
        infer, rate, or recommend. Seniority is the level of the most recent role,
        in the résumé's own words where possible. Target job titles are the roles
        the résumé says the person wants, or else their most recent titles.
        """;

    private static readonly object Schema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[]
        {
            "fullName", "seniority", "yearsOfExperience", "experienceSummary", "skills", "targetJobTitles"
        },
        properties = new
        {
            fullName = new { type = new[] { "string", "null" } },
            seniority = new { type = new[] { "string", "null" } },
            yearsOfExperience = new { type = new[] { "integer", "null" } },
            experienceSummary = new
            {
                type = new[] { "string", "null" },
                description = "Two or three sentences in the third person, drawn from the résumé."
            },
            skills = new { type = "array", items = new { type = "string" } },
            targetJobTitles = new { type = "array", items = new { type = "string" } }
        }
    };

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
                List(root, "targetJobTitles"));
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

    private static bool IsTransient(HttpStatusCode status) =>
        status == HttpStatusCode.TooManyRequests || (int)status >= 500;
}
