namespace Seeka.Engines.Profiles;

public sealed record Volunteering(
    string Organization,
    string? Role,
    string? StartMonth,
    string? EndMonth,
    bool Current,
    string? Description);
