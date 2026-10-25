namespace Seeka.Managers.Access.Accounts;

/// <summary>Registration values after validation: trimmed, phone in E.164.</summary>
public sealed record NewAccount(
    string Email,
    string Password,
    string DisplayName,
    string PhoneNumber);
