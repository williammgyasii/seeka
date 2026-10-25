namespace Seeka.Managers.Access.Outreach;

public sealed record DraftChanges(
    string? Subject,
    string Body,
    bool IsApproved,
    DateTimeOffset UpdatedAt);
