using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.Persistence;
using Seeka.ResourceAccess.Persistence.Entities;

namespace Seeka.Backend.Tests;

public sealed class AccountSessionTests
{
    [Fact]
    public async Task Session_reports_onboarding_complete_once_a_candidate_profile_exists()
    {
        await using var services = BuildServices();
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SeekaDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var accounts = new AccountAccess(
            users,
            scope.ServiceProvider.GetRequiredService<SignInManager<AppUser>>(),
            db);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = "ama@example.com",
            Email = "ama@example.com",
            DisplayName = "Ama"
        };
        Assert.True((await users.CreateAsync(user, "Str0ng!Passw0rd")).Succeeded);
        var workspace = new Workspace { Id = Guid.NewGuid(), Name = "Ama's workspace" };
        db.Workspaces.Add(workspace);
        db.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceRole.Owner
        });
        await db.SaveChangesAsync();

        var before = await accounts.FindSessionAsync(user.Id, default);

        db.CandidateProfiles.Add(new CandidateProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            FullName = "Ama Mensah"
        });
        await db.SaveChangesAsync();
        var after = await accounts.FindSessionAsync(user.Id, default);

        Assert.False(before!.OnboardingComplete);
        Assert.True(after!.OnboardingComplete);
    }

    private static ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        var databaseName = Guid.NewGuid().ToString();
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddDbContext<SeekaDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<SeekaDbContext>()
            .AddSignInManager();
        return services.BuildServiceProvider();
    }
}
