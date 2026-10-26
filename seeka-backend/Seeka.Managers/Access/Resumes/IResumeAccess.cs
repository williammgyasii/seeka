using Seeka.Engines.Resumes;
using Seeka.Managers.Contracts.Resumes;

namespace Seeka.Managers.Access.Resumes;

public interface IResumeAccess
{
    Task<StoredResume?> FindByKeyAsync(Guid workspaceId, Guid idempotencyKey, CancellationToken cancellationToken);

    /// <summary>Inserts a Pending row; false when the workspace already holds the key.</summary>
    Task<bool> ClaimAsync(NewResume resume, CancellationToken cancellationToken);

    Task ReleaseAsync(Guid resumeId, CancellationToken cancellationToken);

    /// <summary>
    /// Records the result and makes this résumé current. Returns the ids of the
    /// résumés it replaced, whose files are no longer needed.
    /// </summary>
    Task<IReadOnlyList<Guid>> CompleteAsync(
        Guid resumeId,
        ResumeStatus status,
        ResumeDraft? draft,
        CancellationToken cancellationToken);

    Task<StoredResume?> FindCurrentAsync(Guid workspaceId, CancellationToken cancellationToken);
}
