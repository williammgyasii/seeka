using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;

namespace Seeka.Api.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/health")]
    [HttpGet(SeekaApiRoutes.Health)]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
