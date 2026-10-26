using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Seeka.Api.Http;
using Seeka.Engines.Resumes;
using Seeka.Managers.Contracts.Resumes;
using Seeka.Managers.Resumes;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/candidate/resume")]
public sealed class ResumeController(ResumeManager resumes) : ControllerBase
{
    [HttpPost]
    [EnableRateLimiting(ApiRateLimits.Resume)]
    [RequestSizeLimit(ResumeUploadPolicy.MaxBytes + 64 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile? file,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var upload = new ResumeUpload(
            Guid.TryParse(idempotencyKey, out var key) ? key : null,
            file?.FileName,
            file is null ? [] : await ReadBoundedAsync(file, cancellationToken));
        var outcome = await resumes.UploadAsync(User.RequireUserId(), upload, cancellationToken);

        if (outcome.Rejection is { } rejection)
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { [rejection.Field] = [rejection.Message] }));
        if (outcome.InProgress)
            return Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "This upload is still being processed.");
        return Ok(outcome.Resume);
    }

    [HttpGet]
    public async Task<IActionResult> Current(CancellationToken cancellationToken) =>
        await resumes.FindCurrentAsync(User.RequireUserId(), cancellationToken) is { } resume
            ? Ok(resume)
            : NotFound();

    [HttpGet("file")]
    public async Task<IActionResult> Download(CancellationToken cancellationToken) =>
        await resumes.OpenCurrentFileAsync(User.RequireUserId(), cancellationToken) is { } file
            ? File(file.Content, "application/pdf", file.FileName)
            : NotFound();

    /// <summary>Reads one byte past the limit, so the policy can reject without buffering a huge body.</summary>
    private static async Task<byte[]> ReadBoundedAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var buffer = new byte[Math.Min(file.Length, ResumeUploadPolicy.MaxBytes + 1)];
        await using var stream = file.OpenReadStream();
        await stream.ReadExactlyAsync(buffer, cancellationToken);
        return buffer;
    }
}
