using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record PreferencesStep(SearchPreferences Preferences) : ProfileStep
{
    public override string Name => OnboardingSteps.Preferences;
}
