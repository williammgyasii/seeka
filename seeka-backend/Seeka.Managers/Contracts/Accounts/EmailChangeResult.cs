namespace Seeka.Managers.Contracts.Accounts;

public sealed record EmailChangeResult(
    VerificationEmailOutcome Outcome,
    IReadOnlyDictionary<string, string[]> Errors)
{
    public static EmailChangeResult Of(VerificationEmailOutcome outcome) => new(outcome, new Dictionary<string, string[]>());

    public static EmailChangeResult Invalid(string message) =>
        new(VerificationEmailOutcome.Rejected, new Dictionary<string, string[]> { ["email"] = [message] });
}
