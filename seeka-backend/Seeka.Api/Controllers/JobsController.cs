using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;
using Seeka.Managers.Contracts.Jobs;
using Seeka.Managers.JobSearch;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/jobs")]
public sealed class JobsController(JobSearchManager jobSearch) : ControllerBase
{
    [HttpPost("import")]
    public async Task<IActionResult> Import(ImportJobRequest request, CancellationToken cancellationToken) =>
        Created("/api/jobs", await jobSearch.ImportAsync(User.RequireUserId(), request, cancellationToken));

    [HttpGet("{savedJobId:guid}")]
    public async Task<IActionResult> Get(Guid savedJobId, CancellationToken cancellationToken)
    {
        var detail = await jobSearch.FindJobAsync(User.RequireUserId(), savedJobId, cancellationToken);
        return detail is null ? NotFound() : Ok(detail);
    }
}
