using Seeka.Engines.Profiles;
using Seeka.Managers.Contracts.Candidates;

namespace Seeka.Managers.Access.Candidates;

public sealed record StoredProfile(
    AboutYou About,
    ProfileDetails Details,
    SearchPreferences? Preferences,
    string[] CompletedSteps,
    DateTimeOffset? OnboardingCompletedAt)
{
    public static StoredProfile Empty { get; } = new(
        new AboutYou("", null, "", "", null, 0, null, [], [], []),
        ProfileDetails.Empty,
        null,
        [],
        null);
}
