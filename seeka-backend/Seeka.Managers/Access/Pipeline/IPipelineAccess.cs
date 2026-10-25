using Seeka.Engines.Pipeline;
using Seeka.Managers.Contracts.Jobs;

namespace Seeka.Managers.Access.Pipeline;

public interface IPipelineAccess
{
    Task<IReadOnlyList<PipelineSummary>> ListAsync(
        Guid workspaceId,
        CancellationToken cancellationToken);

    Task<JobDetailResponse?> FindDetailAsync(
        Guid workspaceId,
        Guid savedJobId,
        CancellationToken cancellationToken);

    Task<bool> ChangeStageAsync(
        Guid workspaceId,
        Guid savedJobId,
        PipelineStage stage,
        ActivityEntry activity,
        CancellationToken cancellationToken);

    Task<bool> AppendActivityAsync(
        Guid workspaceId,
        Guid savedJobId,
        ActivityEntry activity,
        CancellationToken cancellationToken);
}
