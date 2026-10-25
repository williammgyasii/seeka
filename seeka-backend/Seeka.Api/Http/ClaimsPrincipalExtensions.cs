using System.Security.Claims;

namespace Seeka.Api.Http;

public static class ClaimsPrincipalExtensions
{
    public static Guid RequireUserId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            ? userId
            : throw new UnauthorizedAccessException("The session has no user identifier.");
}
