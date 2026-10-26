using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;
using Seeka.Managers.Contracts.Resumes;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeResumeAccess : IResumeAccess
{
    private readonly List<Row> rows = [];

    public IReadOnlyList<Row> Rows => rows;

    public Task<StoredResume?> FindByKeyAsync(Guid workspaceId, Guid idempotencyKey, CancellationToken cancellationToken) =>
        Task.FromResult(rows.SingleOrDefault(row => row.WorkspaceId == workspaceId && row.Key == idempotencyKey)?.Stored());

    public Task<bool> ClaimAsync(NewResume resume, CancellationToken cancellationToken)
    {
        if (rows.Any(row => row.WorkspaceId == resume.WorkspaceId && row.Key == resume.IdempotencyKey))
            return Task.FromResult(false);
        rows.Add(new Row(resume.Id, resume.WorkspaceId, resume.IdempotencyKey, resume.FileName, resume.ClaimedAt));
        return Task.FromResult(true);
    }

    public Task ReleaseAsync(Guid resumeId, CancellationToken cancellationToken)
    {
        rows.RemoveAll(row => row.Id == resumeId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Guid>> CompleteAsync(
        Guid resumeId,
        ResumeStatus status,
        ResumeDraft? draft,
        CancellationToken cancellationToken)
    {
        var row = rows.Single(item => item.Id == resumeId);
        row.Status = status;
        row.Draft = draft;
        var replaced = rows
            .Where(item => item.WorkspaceId == row.WorkspaceId && item.Id != row.Id && item.Current)
            .ToList();
        replaced.ForEach(item => item.Current = false);
        row.Current = true;
        return Task.FromResult<IReadOnlyList<Guid>>(replaced.Select(item => item.Id).ToList());
    }

    public Task<StoredResume?> FindCurrentAsync(Guid workspaceId, CancellationToken cancellationToken) =>
        Task.FromResult(rows.SingleOrDefault(row => row.WorkspaceId == workspaceId && row.Current)?.Stored());

    public void SeedPending(Guid workspaceId, Guid key, DateTimeOffset claimedAt) =>
        rows.Add(new Row(Guid.NewGuid(), workspaceId, key, "old.pdf", claimedAt));

    internal sealed class Row(Guid id, Guid workspaceId, Guid key, string fileName, DateTimeOffset claimedAt)
    {
        public Guid Id { get; } = id;
        public Guid WorkspaceId { get; } = workspaceId;
        public Guid Key { get; } = key;
        public ResumeStatus Status { get; set; } = ResumeStatus.Pending;
        public ResumeDraft? Draft { get; set; }
        public bool Current { get; set; }

        public StoredResume Stored() => new(Id, WorkspaceId, fileName, claimedAt, Status, Draft);
    }
}
