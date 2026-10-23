using Seeka.Domain.Assessments;

namespace Seeka.Backend.Tests;

public sealed class EligibilityEngineTests
{
    private static readonly Uri SourceUrl = new("https://example.com/jobs/123");

    [Fact]
    public void Explicit_Ghana_inclusion_is_eligible()
    {
        var result = EligibilityEngine.Assess(
            "Ghana",
            "This remote role is open to candidates based in Ghana, Kenya, or Nigeria.",
            SourceUrl);

        Assert.Equal(EligibilityStatus.Eligible, result.Status);
        Assert.Contains(result.Evidence, item => item.Excerpt.Contains("Ghana"));
        Assert.Equal(AssessmentOrigin.Rules, result.Origin);
    }

    [Fact]
    public void Remote_without_hiring_geography_is_unknown()
    {
        var result = EligibilityEngine.Assess(
            "Ghana",
            "Remote Administrative Assistant. Work with our distributed team.",
            SourceUrl);

        Assert.Equal(EligibilityStatus.Unknown, result.Status);
        Assert.Contains("does not establish", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Explicit_incompatible_residence_is_ineligible()
    {
        var result = EligibilityEngine.Assess(
            "Ghana",
            "Remote within the United States. Applicants must reside in the US.",
            SourceUrl);

        Assert.Equal(EligibilityStatus.Ineligible, result.Status);
        Assert.NotEmpty(result.Evidence);
    }

    [Fact]
    public void Conflicting_location_evidence_needs_confirmation()
    {
        var result = EligibilityEngine.Assess(
            "Ghana",
            "Open to candidates in Ghana. Applicants must reside in the United Kingdom.",
            SourceUrl);

        Assert.Equal(EligibilityStatus.NeedsConfirmation, result.Status);
        Assert.True(result.Evidence.Count >= 2);
    }
}
