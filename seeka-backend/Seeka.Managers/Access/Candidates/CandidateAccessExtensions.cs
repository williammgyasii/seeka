using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Access.Candidates;

public static class CandidateAccessExtensions
{
    public static async Task<CandidateSetupResponse> GetRequiredSetupAsync(
        this ICandidateAccess candidates,
        Guid workspaceId,
        CancellationToken cancellationToken) =>
        await candidates.FindSetupAsync(workspaceId, cancellationToken)
        ?? throw new InvalidOperationException(
            "Complete the candidate profile and job preferences before importing a job.");
}
