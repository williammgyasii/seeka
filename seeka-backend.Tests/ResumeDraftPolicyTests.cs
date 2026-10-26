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
    public void Truncates_a_long_summary() =>
        Assert.Equal(
            ResumeDraftPolicy.MaxSummaryLength,
            ResumeDraftPolicy.From(Blank with { ExperienceSummary = new string('a', 5_000) })
                .ExperienceSummary!.Length);
}
