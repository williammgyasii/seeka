using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seeka.Managers.Access.Accounts;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.Persistence;
using Seeka.ResourceAccess.Persistence.Entities;

namespace Seeka.Backend.Tests;

public sealed class AccountSessionTests : IAsyncLifetime
{
    private ServiceProvider services = null!;
    private AsyncServiceScope scope;
    private SeekaDbContext db = null!;
    private UserManager<AppUser> users = null!;
    private AccountAccess accounts = null!;

    public Task InitializeAsync()
    {
        services = BuildServices();
        scope = services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext =
            new DefaultHttpContext { RequestServices = scope.ServiceProvider };
        db = scope.ServiceProvider.GetRequiredService<SeekaDbContext>();
        users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        accounts = new AccountAccess(
            users,
            scope.ServiceProvider.GetRequiredService<SignInManager<AppUser>>(),
            db);
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await scope.DisposeAsync();
        await services.DisposeAsync();
    }

    [Fact]
    public async Task Session_reports_onboarding_complete_only_once_it_is_finished()
    {
        var (user, workspace) = await CreateUserAsync("ama@example.com");
        var profile = new CandidateProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            FullName = "Ama Mensah"
        };

        db.CandidateProfiles.Add(profile);
        await db.SaveChangesAsync();
        var started = await accounts.FindSessionAsync(user.Id, default);
        profile.OnboardingCompletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        var finished = await accounts.FindSessionAsync(user.Id, default);

        Assert.False(started!.OnboardingComplete);
        Assert.True(finished!.OnboardingComplete);
    }

    [Fact]
    public async Task A_valid_token_verifies_the_email()
    {
        var (user, _) = await CreateUserAsync("esi@example.org");

        var token = await accounts.CreateEmailConfirmationTokenAsync(user.Id, default);
        var confirmed = await accounts.ConfirmEmailAsync(user.Id, token, default);

        Assert.True(confirmed);
        Assert.True((await accounts.FindSessionAsync(user.Id, default))!.EmailVerified);
        Assert.True((await accounts.FindEmailAsync(user.Id, default))!.Verified);
    }

    [Fact]
    public async Task An_altered_token_or_unknown_user_does_not_verify()
    {
        var (user, _) = await CreateUserAsync("esi@example.org");
        var token = await accounts.CreateEmailConfirmationTokenAsync(user.Id, default);

        Assert.False(await accounts.ConfirmEmailAsync(user.Id, token + "x", default));
        Assert.False(await accounts.ConfirmEmailAsync(Guid.NewGuid(), token, default));
        Assert.False((await accounts.FindSessionAsync(user.Id, default))!.EmailVerified);
    }

    [Fact]
    public async Task Changing_the_address_replaces_it_and_invalidates_older_links()
    {
        var (user, _) = await CreateUserAsync("esi@exmaple.org");
        var oldToken = await accounts.CreateEmailConfirmationTokenAsync(user.Id, default);

        var change = await accounts.ChangeUnverifiedEmailAsync(user.Id, "esi@example.org", default);

        Assert.Equal(EmailChange.Changed, change);
        var email = await accounts.FindEmailAsync(user.Id, default);
        Assert.Equal("esi@example.org", email!.Email);
        Assert.False(email.Verified);
        Assert.False(await accounts.ConfirmEmailAsync(user.Id, oldToken, default));
        Assert.NotNull(await accounts.PasswordSignInAsync("esi@example.org", Password, default));
    }

    [Fact]
    public async Task Changing_to_an_address_in_use_is_refused()
    {
        await CreateUserAsync("taken@example.org");
        var (user, _) = await CreateUserAsync("esi@exmaple.org");

        var change = await accounts.ChangeUnverifiedEmailAsync(user.Id, "Taken@example.org", default);

        Assert.Equal(EmailChange.InUse, change);
        Assert.Equal("esi@exmaple.org", (await accounts.FindEmailAsync(user.Id, default))!.Email);
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("")]
    public async Task Changing_to_an_invalid_address_is_refused(string address)
    {
        var (user, _) = await CreateUserAsync("esi@exmaple.org");

        Assert.Equal(EmailChange.Invalid, await accounts.ChangeUnverifiedEmailAsync(user.Id, address, default));
    }

    [Fact]
    public async Task A_verified_address_cannot_be_changed_here()
    {
        var (user, _) = await CreateUserAsync("esi@example.org");
        var token = await accounts.CreateEmailConfirmationTokenAsync(user.Id, default);
        await accounts.ConfirmEmailAsync(user.Id, token, default);

        var change = await accounts.ChangeUnverifiedEmailAsync(user.Id, "other@example.org", default);

        Assert.Equal(EmailChange.AlreadyVerified, change);
        Assert.Equal("esi@example.org", (await accounts.FindEmailAsync(user.Id, default))!.Email);
    }

    private const string Password = "Str0ng!Passw0rd";

    private async Task<(AppUser User, Workspace Workspace)> CreateUserAsync(string email)
    {
        var user = new AppUser { Id = Guid.NewGuid(), UserName = email, Email = email, DisplayName = "Esi" };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Workspace" };
        db.Workspaces.Add(workspace);
        db.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceRole.Owner
        });
        await db.SaveChangesAsync();
        return (user, workspace);
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddDbContext<SeekaDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddIdentityCore<AppUser>(options => options.User.RequireUniqueEmail = true)
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SeekaDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        return services.BuildServiceProvider();
    }
}
