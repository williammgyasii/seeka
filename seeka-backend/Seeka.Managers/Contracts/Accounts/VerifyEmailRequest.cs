namespace Seeka.Managers.Contracts.Accounts;

public sealed record VerifyEmailRequest(Guid UserId, string Token);
