using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;
using Seeka.Engines.Profiles;
using Seeka.Managers.Candidates;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/candidate")]
public sealed class CandidateController(CandidateProfileManager profiles) : ControllerBase
{
    [HttpGet("profile")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) =>
        Ok(await profiles.GetAsync(User.RequireUserId(), cancellationToken));

    [HttpPut("profile/about")]
    public Task<IActionResult> SaveAbout(AboutYou about, CancellationToken cancellationToken) =>
        SaveAsync(new AboutStep(about), cancellationToken);

    [HttpPut("profile/experience")]
    public Task<IActionResult> SaveExperience(ExperienceStep step, CancellationToken cancellationToken) =>
        SaveAsync(step, cancellationToken);

    [HttpPut("profile/education")]
    public Task<IActionResult> SaveEducation(EducationStep step, CancellationToken cancellationToken) =>
        SaveAsync(step, cancellationToken);

    [HttpPut("profile/projects")]
    public Task<IActionResult> SaveProjects(ProjectsStep step, CancellationToken cancellationToken) =>
        SaveAsync(step, cancellationToken);

    [HttpPut("profile/preferences")]
    public Task<IActionResult> SavePreferences(SearchPreferences preferences, CancellationToken cancellationToken) =>
        SaveAsync(new PreferencesStep(preferences), cancellationToken);

    [HttpGet("job-titles")]
    public async Task<IActionResult> SearchJobTitles([FromQuery] string? q, CancellationToken cancellationToken) =>
        Ok(await profiles.SearchJobTitlesAsync(q, cancellationToken));

    [HttpGet("pay-bands")]
    public IActionResult PayBands() => Ok(CandidateProfileManager.GetPayBands());

    [HttpPost("onboarding/complete")]
    public async Task<IActionResult> CompleteOnboarding(CancellationToken cancellationToken)
    {
        var missing = await profiles.CompleteOnboardingAsync(User.RequireUserId(), cancellationToken);
        return missing.Count > 0
            ? ValidationProblem(new ValidationProblemDetails(missing.ToDictionary()))
            : NoContent();
    }

    private async Task<IActionResult> SaveAsync(ProfileStep step, CancellationToken cancellationToken)
    {
        var result = await profiles.SaveStepAsync(User.RequireUserId(), step, cancellationToken);
        return result.Profile is null
            ? ValidationProblem(new ValidationProblemDetails(result.Errors.ToDictionary()))
            : Ok(result.Profile);
    }
}
