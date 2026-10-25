using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Seeka.Api.Http;
using Seeka.Managers.Contracts.Accounts;
using Seeka.Managers.Membership;

namespace Seeka.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AccountController(MembershipManager membership) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Auth)]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var creation = await membership.RegisterAsync(request, cancellationToken);
        return creation.Session is null
            ? ValidationProblem(new ValidationProblemDetails(creation.Errors.ToDictionary()))
            : Ok(creation.Session);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Auth)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var session = await membership.SignInAsync(request, cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await membership.SignOutAsync();
        return NoContent();
    }

    [HttpGet("session")]
    [Authorize]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        var session = await membership.FindSessionAsync(User.RequireUserId(), cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }
}
