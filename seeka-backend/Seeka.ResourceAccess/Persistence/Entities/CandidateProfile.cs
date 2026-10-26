namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class CandidateProfile
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string FullName { get; set; } = "";
    public string? Headline { get; set; }
    public string CountryOfResidence { get; set; } = "";
    public string TimeZone { get; set; } = "";
    public string Seniority { get; set; } = "";
    public int YearsOfExperience { get; set; }
    public string ExperienceSummary { get; set; } = "";
    public string[] Skills { get; set; } = [];

    /// <summary>The résumé sections, always read and written whole with the profile.</summary>
    public string? DetailsJson { get; set; }

    public string[] CompletedSteps { get; set; } = [];
    public DateTimeOffset? OnboardingCompletedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
