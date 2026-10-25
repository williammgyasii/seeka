namespace Seeka.Managers.Contracts.Outreach;

public sealed record UpdateDraftRequest(
    string? Subject,
    string Body,
    bool IsApproved);
