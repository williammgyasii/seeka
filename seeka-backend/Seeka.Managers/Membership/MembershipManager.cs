using Seeka.Engines.Accounts;
using Seeka.Engines.Intake;
using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Email;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Contracts.Accounts;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Membership;

public sealed class MembershipManager(
    IAccountAccess accounts,
    IMembershipAccess memberships,
    ICandidateAccess candidates,
    IEmailAccess email,
    AppLinks links,
    TimeProvider timeProvider)
{
    public async Task<VerificationEmailOutcome> ResendVerificationEmailAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var account = await accounts.FindEmailAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("The account could not be found.");
        if (account.Verified) return VerificationEmailOutcome.AlreadyVerified;
        return await SendVerificationAsync(userId, account, cancellationToken);
    }

    public async Task<EmailChangeResult> ChangeEmailAsync(
        Guid userId,
        ChangeEmailRequest request,
        CancellationToken cancellationToken)
    {
        var change = await accounts.ChangeUnverifiedEmailAsync(userId, request.Email.Trim(), cancellationToken);
        switch (change)
        {
            case EmailChange.Invalid:
                return EmailChangeResult.Invalid("Enter a valid email address.");
            case EmailChange.InUse:
                return EmailChangeResult.Invalid("Another account already uses this email.");
            case EmailChange.AlreadyVerified:
                return EmailChangeResult.Of(VerificationEmailOutcome.AlreadyVerified);
            case EmailChange.NotFound:
                throw new UnauthorizedAccessException("The account could not be found.");
        }

        var account = await accounts.FindEmailAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("The account could not be found.");
        return EmailChangeResult.Of(await SendVerificationAsync(userId, account, cancellationToken));
    }

    public Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(request.Token)
            ? Task.FromResult(false)
            : accounts.ConfirmEmailAsync(request.UserId, request.Token, cancellationToken);

    public async Task<bool> IsEmailVerifiedAsync(Guid userId, CancellationToken cancellationToken) =>
        (await accounts.FindEmailAsync(userId, cancellationToken))?.Verified ?? false;

    private async Task<VerificationEmailOutcome> SendVerificationAsync(
        Guid userId,
        AccountEmail account,
        CancellationToken cancellationToken)
    {
        var token = await accounts.CreateEmailConfirmationTokenAsync(userId, cancellationToken);
        var link = AccountEmails.VerificationLink(links.PublicUrl, userId, token);
        var sent = await email.SendAsync(
            new OutgoingEmail(account.Email, AccountEmails.Verification(account.DisplayName, link)),
            cancellationToken);
        return sent ? VerificationEmailOutcome.Sent : VerificationEmailOutcome.NotSent;
    }

    public async Task<AccountCreation> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            errors["displayName"] = ["Enter your full name."];
        var phoneNumber = PhoneNumberPolicy.ToE164(request.PhoneNumber ?? "");
        if (phoneNumber is null)
            errors["phoneNumber"] = ["Enter a valid phone number, including the country code."];
        if (errors.Count > 0) return AccountCreation.Rejected(errors);

        var creation = await accounts.CreateWithPersonalWorkspaceAsync(
            new NewAccount(
                request.Email.Trim(),
                request.Password,
                request.DisplayName.Trim(),
                phoneNumber!),
            timeProvider.GetUtcNow(),
            cancellationToken);
        if (creation.Session is null) return creation;

        var userId = creation.Session.UserId;
        await accounts.SignInAsync(userId, cancellationToken);
        await SendVerificationAsync(
            userId,
            new AccountEmail(creation.Session.Email, creation.Session.DisplayName, false),
            cancellationToken);
        return creation;
    }

    public async Task<SessionResponse?> SignInAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var userId = await accounts.PasswordSignInAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);
        return userId is null
            ? null
            : await accounts.FindSessionAsync(userId.Value, cancellationToken);
    }

    public Task SignOutAsync() => accounts.SignOutAsync();

    public Task<SessionResponse?> FindSessionAsync(Guid userId, CancellationToken cancellationToken) =>
        accounts.FindSessionAsync(userId, cancellationToken);

    public async Task<CandidateSetupResponse?> FindCandidateSetupAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await candidates.FindSetupAsync(workspace.WorkspaceId, cancellationToken);
    }

    public async Task<CandidateSetupResponse> SaveCandidateSetupAsync(
        Guid userId,
        CandidateSetupRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var normalized = request with
        {
            FullName = request.FullName.Trim(),
            CountryOfResidence = request.CountryOfResidence.Trim(),
            TimeZone = request.TimeZone.Trim(),
            Seniority = request.Seniority.Trim(),
            ExperienceSummary = request.ExperienceSummary.Trim(),
            Skills = IntakeRules.CleanList(request.Skills),
            TargetJobTitles = IntakeRules.CleanList(request.TargetJobTitles),
            EmploymentTypes = IntakeRules.CleanList(request.EmploymentTypes),
            AvailableWorkingHours = request.AvailableWorkingHours.Trim(),
            CompensationCurrency = IntakeRules.Currency(request.CompensationCurrency)
        };
        return await candidates.SaveSetupAsync(
            workspace.WorkspaceId,
            normalized,
            timeProvider.GetUtcNow(),
            cancellationToken);
    }

    private static void Validate(CandidateSetupRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.CountryOfResidence) ||
            string.IsNullOrWhiteSpace(request.TimeZone) ||
            request.TargetJobTitles.Length == 0)
            throw new ArgumentException("Name, residence, timezone, and a target role are required.");
        if (request.YearsOfExperience < 0)
            throw new ArgumentOutOfRangeException(nameof(request.YearsOfExperience));
        if (request.MinimumCompensation is not null &&
            string.IsNullOrWhiteSpace(request.CompensationCurrency))
            throw new ArgumentException("Compensation currency is required with a minimum.");
    }
}
