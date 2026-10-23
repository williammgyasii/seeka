using Seeka.Domain.Assessments;

namespace Seeka.Backend.Tests;

public sealed class MatchEngineTests
{
    private static readonly CandidateSnapshot Candidate = new(
        ["Administrative Assistant", "HR Coordinator"],
        ["calendar management", "recruitment", "Microsoft Office"],
        "Mid-level",
        5,
        "Africa/Accra",
        2500,
        "USD");

    [Fact]
    public void Assessment_exposes_versioned_components_and_missing_skills()
    {
        var job = new JobSnapshot(
            "Executive Assistant",
            ["calendar management", "Microsoft Office", "Salesforce"],
            "Mid-level",
            "UTC hours",
            3000,
            4000,
            "USD");

        var result = MatchEngine.Assess(Candidate, job);

        Assert.Equal("v1", result.ModelVersion);
        Assert.Equal(6, result.Components.Count);
        Assert.InRange(result.TotalScore, 0, 100);
        Assert.Contains("Salesforce", result.MissingRequirements);
    }

    [Fact]
    public void Undisclosed_salary_remains_unknown_and_earns_no_points()
    {
        var job = new JobSnapshot(
            "Administrative Assistant",
            ["calendar management"],
            "Mid-level",
            "UTC hours",
            null,
            null,
            null);

        var result = MatchEngine.Assess(Candidate, job);
        var compensation = Assert.Single(
            result.Components,
            component => component.Name == "Compensation compatibility");

        Assert.Equal(Compatibility.Unknown, compensation.Compatibility);
        Assert.Equal(0, compensation.EarnedPoints);
    }
}
