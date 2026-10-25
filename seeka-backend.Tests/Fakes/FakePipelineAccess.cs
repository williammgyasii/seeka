using Seeka.Engines.Pipeline;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Jobs;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakePipelineAccess : IPipelineAccess
{
    public Guid? ListedWorkspaceId { get; private set; }

    public Task<IReadOnlyList<PipelineSummary>> ListAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        ListedWorkspaceId = workspaceId;
        return Task.FromResult<IReadOnlyList<PipelineSummary>>([]);
    }

    public Task<JobDetailResponse?> FindDetailAsync(
        Guid workspaceId,
        Guid savedJobId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        return Task.FromResult<JobDetailResponse?>(new JobDetailResponse(
            new SavedJobView(savedJobId, workspaceId, Guid.NewGuid(), PipelineStage.Saved,
                ApplicationStatus.NotApplied, null, now, now),
            new JobPostingView(Guid.NewGuid(), null, "Acme", "Executive Assistant", "", "", "",
                null, null, null, [], null, null, null, null, null, null, now, now, "Unverified"),
            null,
            null,
            [],
            [],
            []));
    }

    public Task<bool> ChangeStageAsync(
        Guid workspaceId,
        Guid savedJobId,
        PipelineStage stage,
        ActivityEntry activity,
        CancellationToken cancellationToken) => Task.FromResult(true);

    public Task<bool> AppendActivityAsync(
        Guid workspaceId,
        Guid savedJobId,
        ActivityEntry activity,
        CancellationToken cancellationToken) => Task.FromResult(true);
}
