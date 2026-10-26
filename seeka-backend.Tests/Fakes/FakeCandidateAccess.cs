using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeCandidateAccess(CandidateSetupResponse? setup) : ICandidateAccess
{
    public Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken) => Task.FromResult(setup);

    public Task<StoredProfile> FindProfileAsync(Guid workspaceId, CancellationToken cancellationToken) =>
        Task.FromResult(StoredProfile.Empty);

    public Task SaveStepAsync(
        Guid workspaceId,
        ProfileStep step,
        string[] completedSteps,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task CompleteOnboardingAsync(
        Guid workspaceId,
        DateTimeOffset completedAt,
        CancellationToken cancellationToken) => throw new NotSupportedException();
}
