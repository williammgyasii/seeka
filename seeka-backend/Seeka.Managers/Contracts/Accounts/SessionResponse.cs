namespace Seeka.Managers.Contracts.Accounts;

public sealed record SessionResponse(
    Guid UserId,
    string Email,
    string DisplayName,
    Guid WorkspaceId,
    bool OnboardingComplete,
    bool EmailVerified);
