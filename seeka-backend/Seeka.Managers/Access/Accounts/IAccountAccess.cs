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
}
