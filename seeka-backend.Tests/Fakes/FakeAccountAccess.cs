using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Contracts.Accounts;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeAccountAccess : IAccountAccess
{
    public const string ValidToken = "valid-token";

    public bool Created { get; private set; }
    public string? CreatedPhoneNumber { get; private set; }
    public Guid? SignedInUserId { get; private set; }
    public AccountEmail? Email { get; set; }
    public EmailChange ChangeResult { get; set; } = EmailChange.Changed;
    public string? ChangedTo { get; private set; }
    public bool Confirmed { get; private set; }

    public Task<AccountCreation> CreateWithPersonalWorkspaceAsync(
        NewAccount account,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        Created = true;
        CreatedPhoneNumber = account.PhoneNumber;
        Email = new AccountEmail(account.Email, account.DisplayName, false);
        return Task.FromResult(AccountCreation.Created(
            new SessionResponse(Guid.NewGuid(), account.Email, account.DisplayName, Guid.NewGuid(), false, false)));
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

    public Task<AccountEmail?> FindEmailAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(Email);

    public Task<string> CreateEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult("a+b/c=");

    public Task<bool> ConfirmEmailAsync(Guid userId, string token, CancellationToken cancellationToken)
    {
        Confirmed = token == ValidToken;
        return Task.FromResult(Confirmed);
    }

    public Task<EmailChange> ChangeUnverifiedEmailAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken)
    {
        if (ChangeResult == EmailChange.Changed)
        {
            ChangedTo = email;
            Email = Email! with { Email = email };
        }
        return Task.FromResult(ChangeResult);
    }
}
