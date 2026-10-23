using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Seeka.Infrastructure;

namespace Seeka.Application;

public sealed record AuthorizedWorkspace(Guid UserId, Guid WorkspaceId);

public sealed class WorkspaceContext(SeekaDbContext db)
{
    public async Task<AuthorizedWorkspace?> ResolveAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(value, out var userId)) return null;
        var membership = await db.WorkspaceMemberships.AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.CreatedAt)
            .Select(item => new { item.UserId, item.WorkspaceId })
            .FirstOrDefaultAsync(cancellationToken);
        return membership is null
            ? null
            : new AuthorizedWorkspace(membership.UserId, membership.WorkspaceId);
    }
}
