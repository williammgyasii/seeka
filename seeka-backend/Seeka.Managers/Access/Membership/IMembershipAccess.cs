namespace Seeka.Managers.Access.Membership;

public interface IMembershipAccess
{
    Task<AuthorizedWorkspace?> FindDefaultWorkspaceAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
