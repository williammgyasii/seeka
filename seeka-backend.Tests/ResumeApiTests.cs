using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Seeka.Backend.Tests.Fakes;

namespace Seeka.Backend.Tests;

public sealed partial class ResumeApiTests : IDisposable
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7 résumé");
    private readonly SeekaApiFactory api = new();

    public void Dispose() => api.Dispose();

    [Fact]
    public async Task Unverified_person_cannot_upload()
    {
        var browser = await RegisterAsync("esi@example.org");

        var response = await UploadAsync(browser, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Upload_returns_the_extracted_draft()
    {
        var browser = await VerifiedAsync();

        var response = await UploadAsync(browser, Guid.NewGuid());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Extracted", body.GetProperty("status").GetString());
        Assert.Equal("cv.pdf", body.GetProperty("fileName").GetString());
        Assert.Equal("Ama Mensah", body.GetProperty("draft").GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task Upload_without_a_key_is_a_field_error()
    {
        var browser = await VerifiedAsync();

        var response = await UploadAsync(browser, null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("idempotencyKey", out _));
        Assert.Empty(api.ResumeFiles.Files);
    }

    [Fact]
    public async Task Upload_of_another_file_type_is_a_field_error()
    {
        var browser = await VerifiedAsync();

        var response = await UploadAsync(browser, Guid.NewGuid(), Encoding.ASCII.GetBytes("PK\u0003\u0004"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("file", out _));
    }

    [Fact]
    public async Task Retried_upload_replays_without_extracting_again()
    {
        var browser = await VerifiedAsync();
        var key = Guid.NewGuid();

        var first = await (await UploadAsync(browser, key)).Content.ReadAsStringAsync();
        var second = await UploadAsync(browser, key);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(first, await second.Content.ReadAsStringAsync());
        Assert.Equal(1, api.Extractor.Calls);
        Assert.Single(api.ResumeFiles.Files);
    }

    [Fact]
    public async Task Duplicate_while_the_first_is_running_is_a_conflict()
    {
        var browser = await VerifiedAsync();
        var release = new TaskCompletionSource();
        api.Extractor.Hold = release.Task;
        var key = Guid.NewGuid();

        var first = UploadAsync(browser, key);
        await api.Extractor.Started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        var duplicate = await UploadAsync(browser, key);
        release.SetResult();

        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await first).StatusCode);
    }

    [Fact]
    public async Task Current_resume_and_file_are_returned_after_upload()
    {
        var browser = await VerifiedAsync();
        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/api/candidate/resume")).StatusCode);
        await UploadAsync(browser, Guid.NewGuid());

        var current = await browser.GetFromJsonAsync<JsonElement>("/api/candidate/resume");
        var file = await browser.GetAsync("/api/candidate/resume/file");

        Assert.Equal("Ama Mensah", current.GetProperty("draft").GetProperty("fullName").GetString());
        Assert.Equal("application/pdf", file.Content.Headers.ContentType!.MediaType);
        Assert.Equal(Pdf, await file.Content.ReadAsByteArrayAsync());
    }

    private static Task<HttpResponseMessage> UploadAsync(HttpClient browser, Guid? key, byte[]? content = null)
    {
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(content ?? Pdf);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(file, "file", "cv.pdf");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/candidate/resume") { Content = form };
        if (key is not null) request.Headers.Add("Idempotency-Key", key.ToString());
        return browser.SendAsync(request);
    }

    private async Task<HttpClient> VerifiedAsync()
    {
        var browser = await RegisterAsync("esi@example.org");
        var match = VerificationLink().Match(api.Email.Sent.Single().Content.Text);
        var verify = await browser.PostAsJsonAsync("/api/auth/verify-email", new
        {
            userId = Guid.Parse(match.Groups["userId"].Value),
            token = Uri.UnescapeDataString(match.Groups["token"].Value)
        });
        Assert.Equal(HttpStatusCode.NoContent, verify.StatusCode);
        return browser;
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

    [GeneratedRegex(@"/verify-email/confirm\?userId=(?<userId>[0-9a-f-]+)&token=(?<token>[^\s]+)")]
    private static partial Regex VerificationLink();
}
