namespace Seeka.Managers.Contracts.Pipeline;

public sealed record ActivityView(
    Guid Id,
    Guid WorkspaceId,
    Guid SavedJobId,
    Guid ActorUserId,
    string Type,
    string Description,
    string? IdempotencyKey,
    DateTimeOffset OccurredAt);
