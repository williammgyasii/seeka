using Microsoft.EntityFrameworkCore;
using Seeka.Managers.Access.Membership;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class MembershipAccess(SeekaDbContext db) : IMembershipAccess
{
    public Task<AuthorizedWorkspace?> FindDefaultWorkspaceAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        db.WorkspaceMemberships.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new AuthorizedWorkspace(item.UserId, item.WorkspaceId))
            .FirstOrDefaultAsync(cancellationToken);
}
