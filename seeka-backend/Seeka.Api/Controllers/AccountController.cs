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
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public async Task<IActionResult> Logout()
    {
        await membership.SignOutAsync();
        return NoContent();
    }

    [HttpGet("session")]
    [Authorize(Policy = AuthPolicies.SignedIn)]
    public async Task<IActionResult> Session(CancellationToken cancellationToken)
    {
        var session = await membership.FindSessionAsync(User.RequireUserId(), cancellationToken);
        return session is null ? Unauthorized() : Ok(session);
    }

    [HttpPost("verification-email")]
    [Authorize(Policy = AuthPolicies.SignedIn)]
    [EnableRateLimiting(ApiRateLimits.Auth)]
    public async Task<IActionResult> ResendVerificationEmail(CancellationToken cancellationToken) =>
        Outcome(await membership.ResendVerificationEmailAsync(User.RequireUserId(), cancellationToken));

    [HttpPut("email")]
    [Authorize(Policy = AuthPolicies.SignedIn)]
    [EnableRateLimiting(ApiRateLimits.Auth)]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await membership.ChangeEmailAsync(User.RequireUserId(), request, cancellationToken);
        return result.Outcome == VerificationEmailOutcome.Rejected
            ? ValidationProblem(new ValidationProblemDetails(result.Errors.ToDictionary()))
            : Outcome(result.Outcome);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EnableRateLimiting(ApiRateLimits.Auth)]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken cancellationToken) =>
        await membership.VerifyEmailAsync(request, cancellationToken)
            ? NoContent()
            : Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "This verification link is invalid or has expired.");

    private IActionResult Outcome(VerificationEmailOutcome outcome) => outcome switch
    {
        VerificationEmailOutcome.Sent => Accepted(),
        VerificationEmailOutcome.AlreadyVerified => Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "This email is already verified."),
        _ => Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "We couldn't send the email. Try again in a minute.")
    };
}
