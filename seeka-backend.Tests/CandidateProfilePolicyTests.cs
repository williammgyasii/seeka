using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class CandidateProfilePolicyTests
{
    private static readonly AboutYou About = new(
        "Ama Mensah", "HR operations lead", "Ghana", "Africa/Accra", "Senior", 8,
        "Runs HR for growing teams.", ["Payroll"], [new LanguageSkill("English", "Native")],
        [new ProfileLink("LinkedIn", "https://linkedin.com/in/ama")]);

    [Fact]
    public void A_complete_about_step_is_valid() =>
        Assert.Empty(CandidateProfilePolicy.CheckAbout(About));

    [Fact]
    public void About_requires_name_residence_and_timezone()
    {
        var errors = CandidateProfilePolicy.CheckAbout(About with
        {
            FullName = " ", CountryOfResidence = "", TimeZone = ""
        });

        Assert.Equal(["countryOfResidence", "fullName", "timeZone"], errors.Keys.Order());
    }

    [Theory]
    [InlineData("http://ama.dev")]
    [InlineData("ama.dev")]
    public void Links_must_be_https(string url) =>
        Assert.Contains("links[0].url", CandidateProfilePolicy.CheckAbout(About with
        {
            Links = [new ProfileLink("Portfolio", url)]
        }).Keys);

    [Fact]
    public void Language_level_comes_from_the_fixed_list() =>
        Assert.Contains("languages[0].level", CandidateProfilePolicy.CheckAbout(About with
        {
            Languages = [new LanguageSkill("French", "Okay")]
        }).Keys);

    [Fact]
    public void Timezone_must_be_a_real_timezone() =>
        Assert.Equal(
            ["Choose a timezone from the list."],
            CandidateProfilePolicy.CheckAbout(About with { TimeZone = "Mars/Olympus" })["timeZone"]);

    [Fact]
    public void Seniority_comes_from_the_fixed_list() =>
        Assert.Equal(
            ["Choose a level from the list."],
            CandidateProfilePolicy.CheckAbout(About with { Seniority = "People Operations Specialist" })["seniority"]);

    [Fact]
    public void Seniority_may_be_left_blank() =>
        Assert.Empty(CandidateProfilePolicy.CheckAbout(About with { Seniority = null }));

    [Fact]
    public void Tidy_matches_the_level_case_and_keeps_an_unknown_value_for_the_check()
    {
        Assert.Equal("Mid-level", CandidateProfilePolicy.Tidy(About with { Seniority = " mid-level " }).Seniority);
        Assert.Equal("Wizard", CandidateProfilePolicy.Tidy(About with { Seniority = "Wizard" }).Seniority);
    }

    [Fact]
    public void Negative_years_are_rejected() =>
        Assert.Contains("yearsOfExperience", CandidateProfilePolicy.CheckAbout(About with { YearsOfExperience = -1 }).Keys);

    [Fact]
    public void A_valid_job_history_passes() =>
        Assert.Empty(CandidateProfilePolicy.CheckExperience(
        [
            Job("Acme", "HR Manager", "2021-06", null, current: true),
            Job("Beta", "HR Officer", "2018-01", "2021-05")
        ]));

    [Fact]
    public void An_empty_job_history_is_valid() =>
        Assert.Empty(CandidateProfilePolicy.CheckExperience([]));

    [Fact]
    public void End_before_start_names_the_end_month() =>
        Assert.Equal(
            ["experience[1].endMonth"],
            CandidateProfilePolicy.CheckExperience(
            [
                Job("Acme", "Analyst", "2022-01", "2023-01"),
                Job("Beta", "Analyst", "2021-06", "2020-01")
            ]).Keys);

    [Fact]
    public void A_current_role_cannot_have_an_end_month() =>
        Assert.Contains(
            "experience[0].endMonth",
            CandidateProfilePolicy.CheckExperience([Job("Acme", "Analyst", "2021-06", "2022-01", current: true)]).Keys);

    [Theory]
    [InlineData("June 2021")]
    [InlineData("2021-00")]
    [InlineData("1900-01")]
    public void Months_must_be_year_and_month(string month) =>
        Assert.Contains(
            "experience[0].startMonth",
            CandidateProfilePolicy.CheckExperience([Job("Acme", "Analyst", month)]).Keys);

    [Fact]
    public void A_job_needs_a_company_and_a_title()
    {
        var errors = CandidateProfilePolicy.CheckExperience([Job(" ", "")]);

        Assert.Equal(["experience[0].company", "experience[0].title"], errors.Keys.Order());
    }

    [Fact]
    public void Too_many_jobs_is_an_error_on_the_list() =>
        Assert.Contains(
            "experience",
            CandidateProfilePolicy.CheckExperience(
                Enumerable.Range(1, ProfileLimits.MaxExperience + 1).Select(n => Job($"Co {n}", "Analyst")).ToArray()).Keys);

    [Fact]
    public void Too_many_or_too_long_highlights_are_errors()
    {
        var tooMany = Job("Acme", "Analyst") with
        {
            Highlights = Enumerable.Range(1, ProfileLimits.MaxHighlights + 1).Select(n => $"Did {n}").ToArray()
        };
        var tooLong = Job("Beta", "Analyst") with { Highlights = [new string('x', ProfileLimits.MaxHighlightLength + 1)] };

        var errors = CandidateProfilePolicy.CheckExperience([tooMany, tooLong]);

        Assert.Contains("experience[0].highlights", errors.Keys);
        Assert.Contains("experience[1].highlights[0]", errors.Keys);
    }

    [Fact]
    public void Education_and_certifications_are_checked_together()
    {
        var errors = CandidateProfilePolicy.CheckEducation(
            [new Education("", "BSc", null, 2016, 2012)],
            [new Certification(" ", "SHRM", 1800)]);

        Assert.Equal(
            ["certifications[0].name", "certifications[0].year", "education[0].endYear", "education[0].school"],
            errors.Keys.Order());
    }

    [Fact]
    public void Projects_and_volunteering_are_checked_together()
    {
        var errors = CandidateProfilePolicy.CheckProjects(
            [new Project("Site", null, "http://ama.dev")],
            [new Volunteering("Red Cross", "Coordinator", "2020-01", "2019-01", false, null)]);

        Assert.Equal(["projects[0].url", "volunteering[0].endMonth"], errors.Keys.Order());
    }

    [Fact]
    public void Tidy_trims_text_dedupes_skills_and_names_levels_from_the_list()
    {
        var tidy = CandidateProfilePolicy.Tidy(About with
        {
            FullName = " Ama Mensah ",
            Headline = " ",
            Skills = ["Payroll ", "payroll", " "],
            Languages = [new LanguageSkill(" Twi ", "fluent")],
            Links = [new ProfileLink(" ", " https://ama.dev ")]
        });

        Assert.Equal("Ama Mensah", tidy.FullName);
        Assert.Null(tidy.Headline);
        Assert.Equal(["Payroll"], tidy.Skills);
        Assert.Equal(new LanguageSkill("Twi", "Fluent"), tidy.Languages[0]);
        Assert.Equal(new ProfileLink(null, "https://ama.dev"), tidy.Links[0]);
    }

    [Fact]
    public void Tidy_trims_job_text_and_drops_blank_highlights()
    {
        var job = CandidateProfilePolicy.Tidy(Job(" Acme ", " Analyst ") with { Highlights = [" Built reports ", " "] });

        Assert.Equal(("Acme", "Analyst"), (job.Company, job.Title));
        Assert.Equal(["Built reports"], job.Highlights);
    }

    private static WorkExperience Job(
        string company,
        string title,
        string? start = null,
        string? end = null,
        bool current = false) =>
        new(company, title, null, start, end, current, []);
}
