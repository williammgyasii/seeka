using Seeka.Engines.Outreach;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Outreach;

namespace Seeka.Managers.Access.Outreach;

public interface IOutreachAccess
{
    Task<ContactView?> AddContactAsync(
        Guid workspaceId,
        Guid savedJobId,
        NewContact contact,
        ActivityEntry activity,
        CancellationToken cancellationToken);

    Task<DraftTarget?> FindDraftTargetAsync(
        Guid workspaceId,
        Guid savedJobId,
        Guid contactId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OutreachDraftView>> SaveDraftsAsync(
        Guid workspaceId,
        Guid savedJobId,
        Guid contactId,
        IReadOnlyList<GeneratedDraft> drafts,
        ActivityEntry activity,
        CancellationToken cancellationToken);

    Task<bool> UpdateDraftAsync(
        Guid workspaceId,
        Guid draftId,
        DraftChanges changes,
        CancellationToken cancellationToken);
}
