using Seeka.Managers.Access.Resumes;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeResumeFileAccess : IResumeFileAccess
{
    public Dictionary<Guid, byte[]> Files { get; } = [];
    public bool FailSaves { get; set; }

    public Task SaveAsync(Guid workspaceId, Guid resumeId, byte[] content, CancellationToken cancellationToken)
    {
        if (FailSaves) throw new IOException("storage unavailable");
        Files[resumeId] = content;
        return Task.CompletedTask;
    }

    public Task<Stream?> OpenAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken) =>
        Task.FromResult<Stream?>(Files.TryGetValue(resumeId, out var content) ? new MemoryStream(content) : null);

    public Task DeleteAsync(Guid workspaceId, Guid resumeId, CancellationToken cancellationToken)
    {
        Files.Remove(resumeId);
        return Task.CompletedTask;
    }
}
