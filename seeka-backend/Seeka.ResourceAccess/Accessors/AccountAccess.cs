using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Contracts.Accounts;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class AccountAccess(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    SeekaDbContext db) : IAccountAccess
{
    public async Task<AccountCreation> CreateWithPersonalWorkspaceAsync(
        NewAccount account,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = account.Email,
            Email = account.Email,
            DisplayName = account.DisplayName,
            PhoneNumber = account.PhoneNumber,
            CreatedAt = createdAt
        };
        var creation = await userManager.CreateAsync(user, account.Password);
        if (!creation.Succeeded)
            return AccountCreation.Rejected(creation.Errors.ToDictionary(
                error => error.Code,
                error => new[] { error.Description }));

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = $"{user.DisplayName}'s workspace",
            PersonalOwnerUserId = user.Id,
            CreatedAt = createdAt
        };
        db.Workspaces.Add(workspace);
        db.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspace.Id,
            UserId = user.Id,
            Role = WorkspaceRole.Owner,
            CreatedAt = createdAt
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return AccountCreation.Created(
            new SessionResponse(user.Id, user.Email, user.DisplayName, workspace.Id, false));
    }

    public async Task SignInAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("The account could not be found.");
        await signInManager.SignInAsync(user, isPersistent: true);
    }

    public async Task<Guid?> PasswordSignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return null;
        var result = await signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: true,
            lockoutOnFailure: true);
        return result.Succeeded ? user.Id : null;
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    public async Task<SessionResponse?> FindSessionAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var workspaceId = await db.WorkspaceMemberships.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => (Guid?)item.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);
        if (workspaceId is null) return null;
        var onboardingComplete = await db.CandidateProfiles.AsNoTracking()
            .AnyAsync(item => item.WorkspaceId == workspaceId.Value, cancellationToken);
        return new SessionResponse(
            user.Id,
            user.Email!,
            user.DisplayName,
            workspaceId.Value,
            onboardingComplete);
    }
}
