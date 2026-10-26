using Seeka.Engines.Profiles;

namespace Seeka.Engines.Resumes;

public sealed record ResumeDraft(
    string? FullName,
    string? Seniority,
    int? YearsOfExperience,
    string? ExperienceSummary,
    string[] Skills,
    string[] TargetJobTitles)
{
    public string? Headline { get; init; }

    /// <summary>Empty for drafts stored before sections were extracted.</summary>
    public ProfileDetails Details { get; init; } = ProfileDetails.Empty;
}
