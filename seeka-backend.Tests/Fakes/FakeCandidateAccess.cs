using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeCandidateAccess(CandidateSetupResponse? setup) : ICandidateAccess
{
    public Guid? SavedWorkspaceId { get; private set; }
    public CandidateSetupRequest? Saved { get; private set; }

    public Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken) => Task.FromResult(setup);

    public Task<CandidateSetupResponse> SaveSetupAsync(
        Guid workspaceId,
        CandidateSetupRequest request,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        SavedWorkspaceId = workspaceId;
        Saved = request;
        return Task.FromResult(TestData.GhanaSetup(workspaceId));
    }
}
