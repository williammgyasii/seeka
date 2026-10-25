using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Backend.Tests.Fakes;

internal static class TestData
{
    public static CandidateSetupResponse GhanaSetup(Guid workspaceId) =>
        new(
            new CandidateProfileView(
                Guid.NewGuid(),
                workspaceId,
                "Ama Mensah",
                "Ghana",
                "Africa/Accra",
                "Mid-level",
                5,
                "5 years supporting executives.",
                ["calendar management"],
                DateTimeOffset.UtcNow),
            new JobPreferencesView(
                Guid.NewGuid(),
                workspaceId,
                ["Executive Assistant"],
                ["Full-time"],
                "09:00-17:00 UTC",
                null,
                null,
                DateTimeOffset.UtcNow));
}
