using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Seeka.Api.Http;
using Seeka.Managers.Contracts.Outreach;
using Seeka.Managers.Outreach;

namespace Seeka.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class OutreachController(OutreachManager outreach) : ControllerBase
{
    [HttpPost("jobs/{savedJobId:guid}/contacts")]
    public async Task<IActionResult> AddContact(
        Guid savedJobId,
        AddContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await outreach.AddContactAsync(User.RequireUserId(), savedJobId, request, cancellationToken);
        return contact is null ? NotFound() : Created($"/api/jobs/{savedJobId}", contact);
    }

    [HttpPost("jobs/{savedJobId:guid}/drafts/{contactId:guid}")]
    public async Task<IActionResult> GenerateDrafts(
        Guid savedJobId,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        var drafts = await outreach.GenerateDraftsAsync(
            User.RequireUserId(),
            savedJobId,
            contactId,
            cancellationToken);
        return drafts is null ? NotFound() : Ok(drafts);
    }

    [HttpPut("drafts/{draftId:guid}")]
    public async Task<IActionResult> UpdateDraft(
        Guid draftId,
        UpdateDraftRequest request,
        CancellationToken cancellationToken) =>
        await outreach.UpdateDraftAsync(User.RequireUserId(), draftId, request, cancellationToken)
            ? NoContent()
            : NotFound();
}
