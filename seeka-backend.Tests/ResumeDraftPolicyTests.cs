using Seeka.Engines.Profiles;
using Seeka.Engines.Resumes;

namespace Seeka.Backend.Tests;

public sealed class ResumeDraftPolicyTests
{
    private static readonly ResumeFacts Blank = new(null, null, null, null, null, null);

    [Fact]
    public void Trims_stated_facts()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            FullName = "  Ama Mensah ",
            Seniority = " Senior ",
            ExperienceSummary = " Led HR operations. "
        });

        Assert.Equal("Ama Mensah", draft.FullName);
        Assert.Equal("Senior", draft.Seniority);
        Assert.Equal("Led HR operations.", draft.ExperienceSummary);
    }

    [Theory]
    [InlineData("senior", "Senior")]
    [InlineData("People Operations Specialist", null)]
    public void Keeps_seniority_only_when_it_is_a_level_from_the_list(string reported, string? expected) =>
        Assert.Equal(expected, ResumeDraftPolicy.From(Blank with { Seniority = reported }).Seniority);

    [Fact]
    public void Leaves_unstated_facts_blank()
    {
        var draft = ResumeDraftPolicy.From(Blank with { FullName = "   " });

        Assert.Null(draft.FullName);
        Assert.Null(draft.Seniority);
        Assert.Null(draft.YearsOfExperience);
        Assert.Null(draft.ExperienceSummary);
        Assert.Empty(draft.Skills);
        Assert.Empty(draft.TargetJobTitles);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(12, 12)]
    [InlineData(60, 60)]
    [InlineData(-1, null)]
    [InlineData(61, null)]
    public void Keeps_years_only_within_a_working_life(int years, int? expected) =>
        Assert.Equal(expected, ResumeDraftPolicy.From(Blank with { YearsOfExperience = years }).YearsOfExperience);

    [Fact]
    public void Cleans_and_deduplicates_lists()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Skills = [" Recruitment", "recruitment", "", "Payroll "],
            TargetJobTitles = ["HR Manager", "HR manager"]
        });

        Assert.Equal(["Recruitment", "Payroll"], draft.Skills);
        Assert.Equal(["HR Manager"], draft.TargetJobTitles);
    }

    [Fact]
    public void Caps_list_lengths()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Skills = Enumerable.Range(1, 40).Select(n => $"Skill {n}").ToArray(),
            TargetJobTitles = Enumerable.Range(1, 12).Select(n => $"Title {n}").ToArray()
        });

        Assert.Equal(ResumeDraftPolicy.MaxSkills, draft.Skills.Length);
        Assert.Equal(ResumeDraftPolicy.MaxTargetJobTitles, draft.TargetJobTitles.Length);
        Assert.Equal("Skill 1", draft.Skills[0]);
    }

    [Fact]
    public void Drops_list_items_too_long_to_be_one_skill() =>
        Assert.Empty(ResumeDraftPolicy.From(Blank with { Skills = [new string('x', 81)] }).Skills);

    [Fact]
    public void Keeps_every_stated_section()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Headline = " HR operations lead ",
            Details = ProfileDetails.Empty with
            {
                Experience = [Job(" Acme Ltd ", " HR Manager ", "2021-06", null, true, [" Ran payroll for 300 staff "])],
                Education = [new Education(" University of Ghana ", "BSc", "Psychology", 2012, 2016)],
                Certifications = [new Certification("SHRM-CP", "SHRM", 2020)],
                Languages = [new LanguageSkill("English", "Native"), new LanguageSkill("Twi", "Fluent")],
                Links = [new ProfileLink("LinkedIn", "https://linkedin.com/in/ama")],
                Projects = [new Project("Onboarding revamp", "Cut onboarding to a week.", null)],
                Volunteering = [new Volunteering("Red Cross", "Coordinator", "2019-01", "2020-01", false, null)]
            }
        });

        Assert.Equal("HR operations lead", draft.Headline);
        var job = Assert.Single(draft.Details.Experience);
        Assert.Equal(("Acme Ltd", "HR Manager", "2021-06", true), (job.Company, job.Title, job.StartMonth, job.Current));
        Assert.Equal(["Ran payroll for 300 staff"], job.Highlights);
        Assert.Equal("University of Ghana", Assert.Single(draft.Details.Education).School);
        Assert.Single(draft.Details.Certifications);
        Assert.Equal(2, draft.Details.Languages.Length);
        Assert.Single(draft.Details.Links);
        Assert.Single(draft.Details.Projects);
        Assert.Single(draft.Details.Volunteering);
    }

    [Fact]
    public void Drops_items_missing_a_required_field()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with
            {
                Experience = [Job("", "HR Manager"), Job("Acme", " ")],
                Education = [new Education(" ", "BSc", null, null, null)],
                Certifications = [new Certification("", "SHRM", null)],
                Languages = [new LanguageSkill(" ", "Native")],
                Projects = [new Project("", "Something", null)],
                Volunteering = [new Volunteering("", "Helper", null, null, false, null)]
            }
        });

        AssertEmpty(draft.Details);
    }

    [Theory]
    [InlineData("Spring 2021")]
    [InlineData("2021")]
    [InlineData("2021-13")]
    [InlineData("06/2021")]
    public void Blanks_a_month_that_is_not_year_and_month_but_keeps_the_job(string month)
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with { Experience = [Job("Acme", "Analyst", month, month)] }
        });

        var job = Assert.Single(draft.Details.Experience);
        Assert.Null(job.StartMonth);
        Assert.Null(job.EndMonth);
    }

    [Fact]
    public void A_current_role_has_no_end_month() =>
        Assert.Null(ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with { Experience = [Job("Acme", "Analyst", "2020-01", "2024-01", true)] }
        }).Details.Experience[0].EndMonth);

    [Theory]
    [InlineData("http://ama.dev")]
    [InlineData("linkedin.com/in/ama")]
    [InlineData("javascript:alert(1)")]
    public void Keeps_only_https_links(string url)
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with
            {
                Links = [new ProfileLink("Portfolio", url)],
                Projects = [new Project("Site", null, url)]
            }
        });

        Assert.Empty(draft.Details.Links);
        Assert.Null(Assert.Single(draft.Details.Projects).Url);
    }

    [Fact]
    public void An_unknown_language_level_is_left_blank() =>
        Assert.Null(ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with { Languages = [new LanguageSkill("French", "B2-ish")] }
        }).Details.Languages[0].Level);

    [Fact]
    public void Caps_section_sizes()
    {
        var draft = ResumeDraftPolicy.From(Blank with
        {
            Details = ProfileDetails.Empty with
            {
                Experience = Enumerable.Range(1, 20)
                    .Select(n => Job($"Company {n}", "Analyst", highlights: Enumerable.Range(1, 12).Select(h => $"Did {h}").ToArray()))
                    .ToArray(),
                Links = Enumerable.Range(1, 9).Select(n => new ProfileLink(null, $"https://example.com/{n}")).ToArray()
            }
        });

        Assert.Equal(ProfileLimits.MaxExperience, draft.Details.Experience.Length);
        Assert.Equal(ProfileLimits.MaxHighlights, draft.Details.Experience[0].Highlights.Length);
        Assert.Equal(ProfileLimits.MaxLinks, draft.Details.Links.Length);
        Assert.Equal("Company 1", draft.Details.Experience[0].Company);
    }

    [Fact]
    public void Years_come_from_job_dates_instead_of_the_models_guess()
    {
        var draft = ResumeDraftPolicy.From(
            Blank with
            {
                YearsOfExperience = 12,
                Details = ProfileDetails.Empty with
                {
                    Experience = [Job("Acme", "Analyst", "2018-01", "2021-01"), Job("Beta", "Analyst", "2020-01", "2022-01")]
                }
            },
            new DateOnly(2026, 10, 25));

        Assert.Equal(4, draft.YearsOfExperience);
    }

    [Fact]
    public void Undated_jobs_keep_the_models_years() =>
        Assert.Equal(12, ResumeDraftPolicy.From(
            Blank with { YearsOfExperience = 12, Details = ProfileDetails.Empty with { Experience = [Job("Acme", "Analyst")] } },
            new DateOnly(2026, 10, 25)).YearsOfExperience);

    [Fact]
    public void Draft_without_sections_has_empty_details() =>
        AssertEmpty(ResumeDraftPolicy.From(Blank).Details);

    private static void AssertEmpty(ProfileDetails details)
    {
        Assert.Empty(details.Experience);
        Assert.Empty(details.Education);
        Assert.Empty(details.Certifications);
        Assert.Empty(details.Languages);
        Assert.Empty(details.Links);
        Assert.Empty(details.Projects);
        Assert.Empty(details.Volunteering);
    }

    private static WorkExperience Job(
        string company,
        string title,
        string? start = null,
        string? end = null,
        bool current = false,
        string[]? highlights = null) =>
        new(company, title, null, start, end, current, highlights ?? []);

    [Fact]
    public void Truncates_a_long_summary() =>
        Assert.Equal(
            ResumeDraftPolicy.MaxSummaryLength,
            ResumeDraftPolicy.From(Blank with { ExperienceSummary = new string('a', 5_000) })
                .ExperienceSummary!.Length);
}
