using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Access.Candidates;

public interface ICandidateAccess
{
    /// <summary>The finished profile and preferences that matching reads, or null before both exist.</summary>
    Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken);

    /// <summary>The profile as saved so far; empty before the first step.</summary>
    Task<StoredProfile> FindProfileAsync(
        Guid workspaceId,
        CancellationToken cancellationToken);

    Task SaveStepAsync(
        Guid workspaceId,
        ProfileStep step,
        string[] completedSteps,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);

    Task CompleteOnboardingAsync(
        Guid workspaceId,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken);
}
