using Seeka.Backend.Tests.Fakes;
using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Contracts.Accounts;
using Seeka.Managers.Membership;

namespace Seeka.Backend.Tests;

public sealed class MembershipManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid WorkspaceId = Guid.NewGuid();

    [Fact]
    public async Task Blank_display_name_is_rejected_before_creating_an_account()
    {
        var accounts = new FakeAccountAccess();
        var manager = Create(accounts);

        var creation = await manager.RegisterAsync(
            new RegisterRequest("ama@example.com", "Str0ng!Passw0rd", " ", "+233241234567"),
            default);

        Assert.Null(creation.Session);
        Assert.Contains("displayName", creation.Errors.Keys);
        Assert.False(accounts.Created);
    }

    [Theory]
    [InlineData("+15550000000")]
    [InlineData("0241234567")]
    [InlineData("")]
    public async Task Invalid_phone_number_is_rejected_before_creating_an_account(string phone)
    {
        var accounts = new FakeAccountAccess();
        var manager = Create(accounts);

        var creation = await manager.RegisterAsync(
            new RegisterRequest("ama@example.com", "Str0ng!Passw0rd", "Ama", phone),
            default);

        Assert.Null(creation.Session);
        Assert.Contains("phoneNumber", creation.Errors.Keys);
        Assert.False(accounts.Created);
    }

    [Fact]
    public async Task Registration_stores_the_phone_in_E164_and_signs_the_new_user_in()
    {
        var accounts = new FakeAccountAccess();
        var manager = Create(accounts);

        var creation = await manager.RegisterAsync(
            new RegisterRequest(" ama@example.com ", "Str0ng!Passw0rd", " Ama ", "+233 24 123 4567"),
            default);

        Assert.NotNull(creation.Session);
        Assert.Equal("ama@example.com", creation.Session.Email);
        Assert.Equal("Ama", creation.Session.DisplayName);
        Assert.Equal("+233241234567", accounts.CreatedPhoneNumber);
        Assert.Equal(creation.Session.UserId, accounts.SignedInUserId);
    }

    [Fact]
    public async Task Registration_sends_a_verification_email_to_the_new_address()
    {
        var email = new FakeEmailAccess();
        var manager = Create(new FakeAccountAccess(), email);

        await manager.RegisterAsync(
            new RegisterRequest("esi@example.org", "Str0ng!Passw0rd", "Esi Owusu", "+233241234567"),
            default);

        var sent = Assert.Single(email.Sent);
        Assert.Equal("esi@example.org", sent.To);
        Assert.Contains("https://app.test/verify-email/confirm?userId=", sent.Content.Text);
        Assert.Contains("token=a%2Bb%2Fc%3D", sent.Content.Text);
    }

    [Fact]
    public async Task Registration_succeeds_when_the_email_provider_fails()
    {
        var accounts = new FakeAccountAccess();
        var manager = Create(accounts, new FakeEmailAccess(succeeds: false));

        var creation = await manager.RegisterAsync(
            new RegisterRequest("esi@example.org", "Str0ng!Passw0rd", "Esi Owusu", "+233241234567"),
            default);

        Assert.NotNull(creation.Session);
        Assert.Equal(creation.Session.UserId, accounts.SignedInUserId);
    }

    [Fact]
    public async Task Resend_sends_to_the_current_unverified_address()
    {
        var email = new FakeEmailAccess();
        var accounts = new FakeAccountAccess { Email = new("esi@example.org", "Esi", false) };

        var outcome = await Create(accounts, email)
            .ResendVerificationEmailAsync(UserId, default);

        Assert.Equal(VerificationEmailOutcome.Sent, outcome);
        Assert.Equal("esi@example.org", Assert.Single(email.Sent).To);
    }

    [Fact]
    public async Task Resend_sends_nothing_once_verified()
    {
        var email = new FakeEmailAccess();
        var accounts = new FakeAccountAccess { Email = new("esi@example.org", "Esi", true) };

        var outcome = await Create(accounts, email)
            .ResendVerificationEmailAsync(UserId, default);

        Assert.Equal(VerificationEmailOutcome.AlreadyVerified, outcome);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task Resend_reports_a_provider_failure()
    {
        var accounts = new FakeAccountAccess { Email = new("esi@example.org", "Esi", false) };

        var outcome = await Create(accounts, new FakeEmailAccess(succeeds: false))
            .ResendVerificationEmailAsync(UserId, default);

        Assert.Equal(VerificationEmailOutcome.NotSent, outcome);
    }

    [Fact]
    public async Task Correcting_the_address_sends_to_the_new_address()
    {
        var email = new FakeEmailAccess();
        var accounts = new FakeAccountAccess { Email = new("esi@exmaple.org", "Esi", false) };

        var result = await Create(accounts, email)
            .ChangeEmailAsync(UserId, new ChangeEmailRequest(" esi@example.org "), default);

        Assert.Equal(VerificationEmailOutcome.Sent, result.Outcome);
        Assert.Equal("esi@example.org", accounts.ChangedTo);
        Assert.Equal("esi@example.org", Assert.Single(email.Sent).To);
    }

    [Theory]
    [InlineData(EmailChange.InUse)]
    [InlineData(EmailChange.Invalid)]
    public async Task Correcting_to_an_unusable_address_returns_an_email_error(EmailChange change)
    {
        var email = new FakeEmailAccess();
        var accounts = new FakeAccountAccess { Email = new("esi@exmaple.org", "Esi", false), ChangeResult = change };

        var result = await Create(accounts, email)
            .ChangeEmailAsync(UserId, new ChangeEmailRequest("taken@example.org"), default);

        Assert.Equal(VerificationEmailOutcome.Rejected, result.Outcome);
        Assert.Contains("email", result.Errors.Keys);
        Assert.Empty(email.Sent);
    }

    [Fact]
    public async Task A_verified_address_cannot_be_corrected()
    {
        var email = new FakeEmailAccess();
        var accounts = new FakeAccountAccess
        {
            Email = new("esi@example.org", "Esi", true),
            ChangeResult = EmailChange.AlreadyVerified
        };

        var result = await Create(accounts, email)
            .ChangeEmailAsync(UserId, new ChangeEmailRequest("other@example.org"), default);

        Assert.Equal(VerificationEmailOutcome.AlreadyVerified, result.Outcome);
        Assert.Empty(email.Sent);
    }

    [Theory]
    [InlineData(FakeAccountAccess.ValidToken, true)]
    [InlineData("altered-token", false)]
    [InlineData("", false)]
    public async Task Verification_succeeds_only_with_a_valid_token(string token, bool expected)
    {
        var accounts = new FakeAccountAccess();

        var verified = await Create(accounts)
            .VerifyEmailAsync(new VerifyEmailRequest(UserId, token), default);

        Assert.Equal(expected, verified);
        Assert.Equal(expected, accounts.Confirmed);
    }

    private static MembershipManager Create(
        FakeAccountAccess accounts,
        FakeEmailAccess? email = null) =>
        new(
            accounts,
            email ?? new FakeEmailAccess(),
            new AppLinks("https://app.test/"),
            TimeProvider.System);
}
