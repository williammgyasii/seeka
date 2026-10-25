namespace Seeka.Managers.Access.Membership;

public static class MembershipAccessExtensions
{
    public static async Task<AuthorizedWorkspace> RequireWorkspaceAsync(
        this IMembershipAccess memberships,
        Guid userId,
        CancellationToken cancellationToken) =>
        await memberships.FindDefaultWorkspaceAsync(userId, cancellationToken)
        ?? throw new UnauthorizedAccessException("The account has no workspace membership.");
}
