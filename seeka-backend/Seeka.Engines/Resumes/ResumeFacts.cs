namespace Seeka.Engines.Resumes;

/// <summary>Facts as the extractor reported them; any field may be missing.</summary>
public sealed record ResumeFacts(
    string? FullName,
    string? Seniority,
    int? YearsOfExperience,
    string? ExperienceSummary,
    IReadOnlyList<string>? Skills,
    IReadOnlyList<string>? TargetJobTitles);
