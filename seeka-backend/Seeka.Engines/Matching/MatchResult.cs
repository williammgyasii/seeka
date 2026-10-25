namespace Seeka.Engines.Matching;

public sealed record MatchResult(
    string ModelVersion,
    int TotalScore,
    IReadOnlyList<MatchComponent> Components,
    IReadOnlyList<string> MissingRequirements);
