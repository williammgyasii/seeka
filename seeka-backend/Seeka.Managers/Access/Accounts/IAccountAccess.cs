using Seeka.Managers.Contracts.Accounts;

namespace Seeka.Managers.Access.Accounts;

public interface IAccountAccess
{
    Task<AccountCreation> CreateWithPersonalWorkspaceAsync(
        NewAccount account,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken);

    Task SignInAsync(Guid userId, CancellationToken cancellationToken);

    Task<Guid?> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken);

    Task SignOutAsync();

    Task<SessionResponse?> FindSessionAsync(Guid userId, CancellationToken cancellationToken);

    Task<AccountEmail?> FindEmailAsync(Guid userId, CancellationToken cancellationToken);

    Task<string> CreateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken);

    /// <summary>Replaces an unverified address and keeps the caller signed in.</summary>
    Task<EmailChange> ChangeUnverifiedEmailAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken);
}
