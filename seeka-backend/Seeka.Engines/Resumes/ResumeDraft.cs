namespace Seeka.Engines.Resumes;

public sealed record ResumeDraft(
    string? FullName,
    string? Seniority,
    int? YearsOfExperience,
    string? ExperienceSummary,
    string[] Skills,
    string[] TargetJobTitles);
