namespace Seeka.Managers.Contracts.Accounts;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string DisplayName,
    string PhoneNumber);
