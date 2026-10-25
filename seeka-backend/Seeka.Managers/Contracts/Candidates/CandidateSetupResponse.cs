namespace Seeka.Managers.Contracts.Candidates;

public sealed record CandidateSetupResponse(
    CandidateProfileView Profile,
    JobPreferencesView Preferences);
