namespace Seeka.Managers.Access.Resumes;

public interface IResumeFileAccess
{
    Task SaveAsync(Guid workspaceId, Guid resumeId, byte[] content, CancellationToken cancellationToken);

    Task<Stream?> OpenAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken);

    /// <summary>Best effort: a failed delete is logged, never thrown.</summary>
    Task DeleteAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken);
}
