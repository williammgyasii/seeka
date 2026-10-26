using Microsoft.Extensions.Logging;
using Seeka.Managers.Access.Resumes;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class LocalResumeFileAccess(string root, ILogger<LocalResumeFileAccess> logger) : IResumeFileAccess
{
    public async Task SaveAsync(Guid workspaceId, Guid resumeId, byte[] content, CancellationToken cancellationToken)
    {
        var path = PathFor(workspaceId, resumeId);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllBytesAsync(path, content, cancellationToken);
    }

    public Task<Stream?> OpenAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken)
    {
        var path = PathFor(workspaceId, resumeId);
        return Task.FromResult<Stream?>(File.Exists(path) ? File.OpenRead(path) : null);
    }

    public Task DeleteAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken)
    {
        try
        {
            File.Delete(PathFor(workspaceId, resumeId));
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not delete résumé {ResumeId}.", resumeId);
        }

        return Task.CompletedTask;
    }

    private string PathFor(Guid workspaceId, Guid resumeId) =>
        Path.Combine(root, ResumeStoragePolicy.Key(workspaceId, resumeId));
}
