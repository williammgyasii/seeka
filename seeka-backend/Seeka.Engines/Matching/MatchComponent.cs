namespace Seeka.Engines.Matching;

public sealed record MatchComponent(
    string Name,
    int EarnedPoints,
    int AvailablePoints,
    Compatibility Compatibility,
    string Explanation);
