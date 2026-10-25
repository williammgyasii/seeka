using Seeka.Backend.Tests.Fakes;
using Seeka.Managers.Contracts.Accounts;
using Seeka.Managers.Contracts.Candidates;
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
        var manager = Create(accounts, new FakeCandidateAccess(null));

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
        var manager = Create(accounts, new FakeCandidateAccess(null));

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
        var manager = Create(accounts, new FakeCandidateAccess(null));

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
    public async Task Candidate_setup_is_saved_normalized_into_the_users_workspace()
    {
        var candidates = new FakeCandidateAccess(null);
        var manager = Create(new FakeAccountAccess(), candidates);

        await manager.SaveCandidateSetupAsync(
            UserId,
            new CandidateSetupRequest(" Ama Mensah ", "Ghana", "Africa/Accra", "Mid-level", 5,
                "Supports executives.", ["Excel", " ", "excel"], ["Executive Assistant"],
                ["Full-time"], "08:00-18:00 GMT", 2500, " usd "),
            default);

        Assert.Equal(WorkspaceId, candidates.SavedWorkspaceId);
        Assert.Equal("Ama Mensah", candidates.Saved!.FullName);
        Assert.Equal(["Excel"], candidates.Saved.Skills);
        Assert.Equal("USD", candidates.Saved.CompensationCurrency);
    }

    private static MembershipManager Create(FakeAccountAccess accounts, FakeCandidateAccess candidates) =>
        new(accounts, new FakeMembershipAccess(WorkspaceId), candidates, TimeProvider.System);
}
