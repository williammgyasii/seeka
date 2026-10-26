namespace Seeka.Engines.Profiles;

/// <summary>The about step: who the candidate is, in their own words.</summary>
public sealed record AboutYou(
    string FullName,
    string? Headline,
    string CountryOfResidence,
    string TimeZone,
    string? Seniority,
    int YearsOfExperience,
    string? ExperienceSummary,
    string[] Skills,
    LanguageSkill[] Languages,
    ProfileLink[] Links);
