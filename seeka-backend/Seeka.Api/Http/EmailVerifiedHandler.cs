using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Seeka.Managers.Membership;

namespace Seeka.Api.Http;

/// <summary>
/// Reads the database rather than a cookie claim, so verifying on one device
/// unlocks every other signed-in device immediately.
/// </summary>
public sealed class EmailVerifiedHandler(MembershipManager membership)
    : AuthorizationHandler<EmailVerifiedRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailVerifiedRequirement requirement)
    {
        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return;
        if (await membership.IsEmailVerifiedAsync(userId, CancellationToken.None))
            context.Succeed(requirement);
    }
}
