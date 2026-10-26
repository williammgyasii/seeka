namespace Seeka.Engines.Profiles;

public sealed record Education(
    string School,
    string? Degree,
    string? FieldOfStudy,
    int? StartYear,
    int? EndYear);
