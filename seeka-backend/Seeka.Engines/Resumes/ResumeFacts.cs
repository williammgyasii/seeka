using Seeka.Engines.Profiles;

namespace Seeka.Engines.Resumes;

/// <summary>Facts as the extractor reported them; any field may be missing.</summary>
public sealed record ResumeFacts(
    string? FullName,
    string? Seniority,
    int? YearsOfExperience,
    string? ExperienceSummary,
    IReadOnlyList<string>? Skills,
    IReadOnlyList<string>? TargetJobTitles)
{
    public string? Headline { get; init; }

    /// <summary>Sections as reported; item text may be blank and months malformed.</summary>
    public ProfileDetails? Details { get; init; }
}
