namespace Seeka.Managers.Contracts.Jobs;

public sealed record MatchView(
    Guid Id,
    Guid WorkspaceId,
    Guid JobPostingId,
    string ModelVersion,
    int TotalScore,
    string ComponentsJson,
    string[] MissingRequirements,
    DateTimeOffset AssessedAt);
