using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class ExperienceYearsTests
{
    private static readonly DateOnly Today = new(2026, 10, 25);

    [Fact]
    public void Overlapping_jobs_count_once() =>
        Assert.Equal(4, ExperienceYears.Total([Job("2018-01", "2021-01"), Job("2020-01", "2022-01")], Today));

    [Fact]
    public void Separate_jobs_add_up() =>
        Assert.Equal(3, ExperienceYears.Total([Job("2015-01", "2017-01"), Job("2019-01", "2020-06")], Today));

    [Fact]
    public void A_current_role_runs_to_today() =>
        Assert.Equal(3, ExperienceYears.Total([Job("2023-01", null, current: true)], Today));

    [Fact]
    public void Undated_jobs_give_no_suggestion() =>
        Assert.Null(ExperienceYears.Total([Job(null, null), Job(null, "2020-01")], Today));

    [Fact]
    public void A_past_job_without_an_end_is_ignored() =>
        Assert.Equal(2, ExperienceYears.Total([Job("2010-01", null), Job("2020-01", "2022-01")], Today));

    [Fact]
    public void No_jobs_give_no_suggestion() =>
        Assert.Null(ExperienceYears.Total([], Today));

    private static WorkExperience Job(string? start, string? end, bool current = false) =>
        new("Acme", "Analyst", null, start, end, current, []);
}
