namespace Seeka.Managers.Access.Resumes;

public sealed record NewResume(
    Guid Id,
    Guid WorkspaceId,
    Guid IdempotencyKey,
    string FileName,
    long SizeBytes,
    DateTimeOffset ClaimedAt);
