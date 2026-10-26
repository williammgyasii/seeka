namespace Seeka.Engines.Profiles;

/// <summary>Months are <c>YYYY-MM</c>; a current role has no end month.</summary>
public sealed record WorkExperience(
    string Company,
    string Title,
    string? Location,
    string? StartMonth,
    string? EndMonth,
    bool Current,
    string[] Highlights);
