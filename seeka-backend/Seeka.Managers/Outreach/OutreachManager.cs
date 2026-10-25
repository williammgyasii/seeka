using Seeka.Engines.Intake;
using Seeka.Engines.Outreach;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Access.Outreach;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Outreach;

namespace Seeka.Managers.Outreach;

public sealed class OutreachManager(
    IMembershipAccess memberships,
    ICandidateAccess candidates,
    IOutreachAccess outreach,
    TimeProvider timeProvider)
{
    public async Task<ContactView?> AddContactAsync(
        Guid userId,
        Guid savedJobId,
        AddContactRequest request,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var contact = new NewContact(
            request.Name.Trim(),
            request.Title.Trim(),
            IntakeRules.OptionalHttpUrl(request.ProfessionalProfileUrl),
            IntakeRules.OptionalText(request.ProfessionalEmail),
            "manual",
            ContactVerificationStatus.Unknown,
            request.RelevanceReason.Trim(),
            now);
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await outreach.AddContactAsync(
            workspace.WorkspaceId,
            savedJobId,
            contact,
            new ActivityEntry(userId, "contact_added", $"Added {contact.Name} as a contact.", now),
            cancellationToken);
    }

    public async Task<IReadOnlyList<OutreachDraftView>?> GenerateDraftsAsync(
        Guid userId,
        Guid savedJobId,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var setup = await candidates.GetRequiredSetupAsync(workspace.WorkspaceId, cancellationToken);
        var target = await outreach.FindDraftTargetAsync(
            workspace.WorkspaceId,
            savedJobId,
            contactId,
            cancellationToken);
        if (target is null) return null;

        var drafts = OutreachEngine.Generate(new OutreachContext(
            setup.Profile.FullName,
            setup.Profile.CountryOfResidence,
            setup.Profile.TimeZone,
            setup.Profile.ExperienceSummary,
            target.JobTitle,
            target.CompanyName,
            target.ContactName));
        return await outreach.SaveDraftsAsync(
            workspace.WorkspaceId,
            savedJobId,
            contactId,
            drafts,
            new ActivityEntry(
                userId,
                "drafts_generated",
                "Generated four grounded outreach drafts for review.",
                timeProvider.GetUtcNow()),
            cancellationToken);
    }

    public async Task<bool> UpdateDraftAsync(
        Guid userId,
        Guid draftId,
        UpdateDraftRequest request,
        CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        return await outreach.UpdateDraftAsync(
            workspace.WorkspaceId,
            draftId,
            new DraftChanges(
                request.Subject?.Trim(),
                request.Body.Trim(),
                request.IsApproved,
                timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
