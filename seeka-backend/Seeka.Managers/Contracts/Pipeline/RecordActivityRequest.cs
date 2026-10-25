namespace Seeka.Managers.Contracts.Pipeline;

public sealed record RecordActivityRequest(
    string Type,
    string Description,
    string? IdempotencyKey);
