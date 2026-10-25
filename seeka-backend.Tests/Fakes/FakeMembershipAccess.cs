using Seeka.Managers.Access.Membership;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeMembershipAccess(Guid? workspaceId) : IMembershipAccess
{
    public Task<AuthorizedWorkspace?> FindDefaultWorkspaceAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        Task.FromResult(workspaceId is null ? null : new AuthorizedWorkspace(userId, workspaceId.Value));
}
