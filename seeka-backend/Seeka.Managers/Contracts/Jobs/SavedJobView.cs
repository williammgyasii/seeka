using Seeka.Engines.Pipeline;

namespace Seeka.Managers.Contracts.Jobs;

public sealed record SavedJobView(
    Guid Id,
    Guid WorkspaceId,
    Guid JobPostingId,
    PipelineStage Stage,
    ApplicationStatus ApplicationStatus,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
