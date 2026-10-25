using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;
using Seeka.Managers.Contracts.Candidates;
using Seeka.Managers.Membership;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/candidate")]
public sealed class CandidateController(MembershipManager membership) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var setup = await membership.FindCandidateSetupAsync(User.RequireUserId(), cancellationToken);
        return setup is null ? NotFound() : Ok(setup);
    }

    [HttpPut]
    public async Task<IActionResult> Save(CandidateSetupRequest request, CancellationToken cancellationToken) =>
        Ok(await membership.SaveCandidateSetupAsync(User.RequireUserId(), request, cancellationToken));
}
