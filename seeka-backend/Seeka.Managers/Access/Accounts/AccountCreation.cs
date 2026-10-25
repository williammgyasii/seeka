using Seeka.Managers.Contracts.Accounts;

namespace Seeka.Managers.Access.Accounts;

public sealed record AccountCreation(
    SessionResponse? Session,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static AccountCreation Created(SessionResponse session) =>
        new(session, new Dictionary<string, string[]>());

    public static AccountCreation Rejected(IReadOnlyDictionary<string, string[]> errors) =>
        new(null, errors);
}
