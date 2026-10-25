namespace Seeka.Managers.Access.Pipeline;

public sealed record ActivityEntry(
    Guid ActorUserId,
    string Type,
    string Description,
    DateTimeOffset OccurredAt,
    string? IdempotencyKey = null);
