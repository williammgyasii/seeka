using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Seeka.Backend.Tests.Fakes;

namespace Seeka.Backend.Tests;

public sealed class ProfileApiTests : IDisposable
{
    private static readonly object About = new
    {
        fullName = "Ama Mensah",
        headline = "HR operations lead",
        countryOfResidence = "Ghana",
        timeZone = "Africa/Accra",
        seniority = "Senior",
        yearsOfExperience = 8,
        experienceSummary = "Runs HR for growing teams.",
        skills = new[] { "Payroll" },
        languages = new[] { new { language = "English", level = "Native" } },
        links = new[] { new { label = "LinkedIn", url = "https://linkedin.com/in/ama" } }
    };

    private static readonly object Experience = new
    {
        experience = new[]
        {
            Job("Acme", "HR Manager", "2020-01", "2022-01"),
            Job("Beta", "HR Officer", "2018-01", "2021-01")
        }
    };

    private static readonly object Preferences = new
    {
        targetJobTitles = new[] { "HR Manager" },
        employmentTypes = new[] { "Full-time" },
        minimumCompensation = 6000m,
        maximumCompensation = 10000m,
        compensationCurrency = "GHS"
    };

    private readonly SeekaApiFactory api = new();

    public void Dispose() => api.Dispose();

    [Fact]
    public async Task A_new_user_has_an_empty_profile_and_no_finished_steps()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var profile = await browser.GetFromJsonAsync<JsonElement>("/api/candidate/profile");

        Assert.Empty(Steps(profile));
        Assert.False(profile.GetProperty("onboardingComplete").GetBoolean());
        Assert.Equal(0, profile.GetProperty("experience").GetArrayLength());
    }

    [Fact]
    public async Task Saving_a_step_stores_it_and_marks_it_finished_without_finishing_onboarding()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/about", About);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Ama Mensah", profile.GetProperty("about").GetProperty("fullName").GetString());
        Assert.Equal("Twi", (await SaveAboutWithLanguage(browser, "Twi")).GetProperty("about")
            .GetProperty("languages")[0].GetProperty("language").GetString());
        Assert.Equal(["about"], Steps(profile));
        Assert.False(await OnboardingCompleteAsync(browser));
    }

    [Fact]
    public async Task Saving_the_same_step_twice_gives_the_same_profile()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var first = await (await browser.PutAsJsonAsync("/api/candidate/profile/experience", Experience))
            .Content.ReadFromJsonAsync<JsonElement>();
        var second = await (await browser.PutAsJsonAsync("/api/candidate/profile/experience", Experience))
            .Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(first.GetProperty("experience").GetRawText(), second.GetProperty("experience").GetRawText());
        Assert.Equal(["experience"], Steps(second));
    }

    [Fact]
    public async Task Years_are_suggested_from_job_dates()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var profile = await (await browser.PutAsJsonAsync("/api/candidate/profile/experience", Experience))
            .Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(4, profile.GetProperty("suggestedYearsOfExperience").GetInt32());
    }

    [Fact]
    public async Task Skip_saves_an_empty_step()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var response = await browser.PutAsJsonAsync(
            "/api/candidate/profile/projects",
            new { projects = Array.Empty<object>(), volunteering = Array.Empty<object>() });

        var profile = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(["projects"], Steps(profile));
    }

    [Fact]
    public async Task Education_saves_schools_and_certifications_together()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var profile = await (await browser.PutAsJsonAsync("/api/candidate/profile/education", new
        {
            education = new[] { new { school = "University of Ghana", degree = "BSc", fieldOfStudy = "Psychology", startYear = 2012, endYear = 2016 } },
            certifications = new[] { new { name = "SHRM-CP", issuer = "SHRM", year = 2020 } }
        })).Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("University of Ghana", profile.GetProperty("education")[0].GetProperty("school").GetString());
        Assert.Equal("SHRM-CP", profile.GetProperty("certifications")[0].GetProperty("name").GetString());
    }

    [Fact]
    public async Task An_invalid_step_names_the_field_and_saves_nothing()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/experience", new
        {
            experience = new[] { Job("Acme", "Analyst", "2021-06", "2020-01") }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("experience[0].endMonth", out _));
        var profile = await browser.GetFromJsonAsync<JsonElement>("/api/candidate/profile");
        Assert.Empty(Steps(profile));
    }

    [Fact]
    public async Task Preferences_need_a_target_role()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/preferences", new
        {
            targetJobTitles = Array.Empty<string>(),
            employmentTypes = Array.Empty<string>(),
            minimumCompensation = (decimal?)null,
            compensationCurrency = (string?)null
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("targetJobTitles", out _));
    }

    [Fact]
    public async Task Finishing_too_early_names_the_missing_steps()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);
        await browser.PutAsJsonAsync("/api/candidate/profile/about", About);

        var response = await browser.PostAsync("/api/candidate/onboarding/complete", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("preferences", out _));
        Assert.False(await OnboardingCompleteAsync(browser));
    }

    [Fact]
    public async Task Finishing_with_about_and_preferences_completes_onboarding()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);
        await browser.PutAsJsonAsync("/api/candidate/profile/about", About);
        await browser.PutAsJsonAsync("/api/candidate/profile/preferences", Preferences);

        var response = await browser.PostAsync("/api/candidate/onboarding/complete", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True(await OnboardingCompleteAsync(browser));
        var profile = await browser.GetFromJsonAsync<JsonElement>("/api/candidate/profile");
        Assert.True(profile.GetProperty("onboardingComplete").GetBoolean());
        var preferences = profile.GetProperty("preferences");
        Assert.Equal("human resources manager", preferences.GetProperty("targetJobTitles")[0].GetString());
        Assert.Equal(10000m, preferences.GetProperty("maximumCompensation").GetDecimal());
        Assert.False(preferences.TryGetProperty("availableWorkingHours", out _));
    }

    [Fact]
    public async Task Pay_that_is_not_a_band_is_rejected()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);
        var preferences = JsonSerializer.SerializeToNode(Preferences)!.AsObject();
        preferences["minimumCompensation"] = 5000m;
        preferences["maximumCompensation"] = 9000m;

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/preferences", preferences);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("minimumCompensation", out _));
    }

    [Fact]
    public async Task A_title_not_on_the_list_is_rejected_by_position()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);
        var preferences = JsonSerializer.SerializeToNode(Preferences)!.AsObject();
        preferences["targetJobTitles"] = new System.Text.Json.Nodes.JsonArray("Chief Vibes Officer");

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/preferences", preferences);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("targetJobTitles[0]", out _));
    }

    [Fact]
    public async Task Signed_in_users_search_job_titles()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var titles = await browser.GetFromJsonAsync<string[]>("/api/candidate/job-titles?q=hr%20manager");

        Assert.Equal("human resources manager", titles![0]);
    }

    [Fact]
    public async Task Job_title_search_needs_a_session()
    {
        var response = await api.CreateBrowser().GetAsync("/api/candidate/job-titles?q=hr");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Pay_bands_come_with_each_country_default_currency()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var pay = await browser.GetFromJsonAsync<JsonElement>("/api/candidate/pay-bands");

        var ghs = pay.GetProperty("byCurrency").GetProperty("GHS");
        Assert.Equal(6, ghs.GetArrayLength());
        Assert.Equal(40000m, ghs[5].GetProperty("minimum").GetDecimal());
        Assert.Equal(JsonValueKind.Null, ghs[5].GetProperty("maximum").ValueKind);
        Assert.Equal("GHS", pay.GetProperty("byCountry").GetProperty("GH").GetString());
        Assert.Equal("USD", pay.GetProperty("byCountry").GetProperty("UG").GetString());
    }

    [Fact]
    public async Task Pay_bands_need_a_session()
    {
        var response = await api.CreateBrowser().GetAsync("/api/candidate/pay-bands");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Each_user_reads_only_their_own_profile()
    {
        var ama = await TestBrowsers.VerifiedAsync(api, "ama@example.org");
        var esi = await TestBrowsers.VerifiedAsync(api, "esi@example.org");
        await ama.PutAsJsonAsync("/api/candidate/profile/about", About);

        var esiProfile = await esi.GetFromJsonAsync<JsonElement>("/api/candidate/profile");

        Assert.Empty(Steps(esiProfile));
        Assert.Equal("", esiProfile.GetProperty("about").GetProperty("fullName").GetString());
    }

    [Fact]
    public async Task An_unknown_step_is_not_found()
    {
        var browser = await TestBrowsers.VerifiedAsync(api);

        var response = await browser.PutAsJsonAsync("/api/candidate/profile/hobbies", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static object Job(string company, string title, string? start, string? end) => new
    {
        company,
        title,
        location = (string?)null,
        startMonth = start,
        endMonth = end,
        current = false,
        highlights = Array.Empty<string>()
    };

    private static async Task<JsonElement> SaveAboutWithLanguage(HttpClient browser, string language)
    {
        var about = JsonSerializer.SerializeToNode(About)!.AsObject();
        about["languages"] = JsonSerializer.SerializeToNode(new[] { new { language, level = "Fluent" } });
        return await (await browser.PutAsJsonAsync("/api/candidate/profile/about", about))
            .Content.ReadFromJsonAsync<JsonElement>();
    }

    private static string[] Steps(JsonElement profile) =>
        profile.GetProperty("completedSteps").EnumerateArray().Select(step => step.GetString()!).ToArray();

    private static async Task<bool> OnboardingCompleteAsync(HttpClient browser) =>
        (await browser.GetFromJsonAsync<JsonElement>("/api/auth/session")).GetProperty("onboardingComplete").GetBoolean();
}
