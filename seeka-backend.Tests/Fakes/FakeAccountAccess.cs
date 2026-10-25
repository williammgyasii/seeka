using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Contracts.Accounts;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeAccountAccess : IAccountAccess
{
    public bool Created { get; private set; }
    public string? CreatedPhoneNumber { get; private set; }
    public Guid? SignedInUserId { get; private set; }

    public Task<AccountCreation> CreateWithPersonalWorkspaceAsync(
        NewAccount account,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        Created = true;
        CreatedPhoneNumber = account.PhoneNumber;
        return Task.FromResult(AccountCreation.Created(
            new SessionResponse(Guid.NewGuid(), account.Email, account.DisplayName, Guid.NewGuid(), false)));
    }

    public Task SignInAsync(Guid userId, CancellationToken cancellationToken)
    {
        SignedInUserId = userId;
        return Task.CompletedTask;
    }

    public Task<Guid?> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken) => Task.FromResult<Guid?>(null);

    public Task SignOutAsync() => Task.CompletedTask;

    public Task<SessionResponse?> FindSessionAsync(
        Guid userId,
        CancellationToken cancellationToken) => Task.FromResult<SessionResponse?>(null);
}
