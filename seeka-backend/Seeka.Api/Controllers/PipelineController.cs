using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;
using Seeka.Engines.Pipeline;
using Seeka.Managers.Contracts.Pipeline;
using Seeka.Managers.JobSearch;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class PipelineController(JobSearchManager jobSearch) : ControllerBase
{
    [HttpGet("pipeline")]
    public async Task<IActionResult> List(CancellationToken cancellationToken) =>
        Ok(await jobSearch.GetPipelineAsync(User.RequireUserId(), cancellationToken));

    [HttpPut("jobs/{savedJobId:guid}/stage/{stage}")]
    public async Task<IActionResult> ChangeStage(
        Guid savedJobId,
        PipelineStage stage,
        CancellationToken cancellationToken) =>
        await jobSearch.ChangeStageAsync(User.RequireUserId(), savedJobId, stage, cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPost("jobs/{savedJobId:guid}/activities")]
    public async Task<IActionResult> RecordActivity(
        Guid savedJobId,
        RecordActivityRequest request,
        CancellationToken cancellationToken) =>
        await jobSearch.RecordActivityAsync(User.RequireUserId(), savedJobId, request, cancellationToken)
            ? Created($"/api/jobs/{savedJobId}", null)
            : NotFound();
}
