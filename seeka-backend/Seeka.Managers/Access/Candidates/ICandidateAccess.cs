using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Access.Candidates;

public interface ICandidateAccess
{
    Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken);

    Task<CandidateSetupResponse> SaveSetupAsync(
        Guid workspaceId,
        CandidateSetupRequest setup,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);
}
