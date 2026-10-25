namespace Seeka.Managers.Contracts.Outreach;

public sealed record OutreachDraftView(
    Guid Id,
    Guid WorkspaceId,
    Guid SavedJobId,
    Guid? ContactId,
    string Channel,
    string? Subject,
    string Body,
    bool IsApproved,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
