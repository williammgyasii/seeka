using Seeka.Engines.Profiles;

namespace Seeka.Backend.Tests;

public sealed class OnboardingStepsTests
{
    [Fact]
    public void Steps_run_in_onboarding_order() =>
        Assert.Equal(["about", "experience", "education", "projects", "preferences"], OnboardingSteps.All);

    [Fact]
    public void About_and_preferences_are_required() =>
        Assert.Equal(["about", "preferences"], OnboardingSteps.Missing([]));

    [Fact]
    public void Optional_steps_are_never_missing() =>
        Assert.Empty(OnboardingSteps.Missing(["preferences", "about"]));

    [Fact]
    public void Reports_only_what_is_still_missing() =>
        Assert.Equal(["preferences"], OnboardingSteps.Missing(["about", "experience"]));

    [Fact]
    public void Marking_a_step_keeps_onboarding_order() =>
        Assert.Equal(["about", "education", "preferences"], OnboardingSteps.Mark(["preferences", "about"], "education"));

    [Fact]
    public void Marking_a_finished_step_again_changes_nothing() =>
        Assert.Equal(["about"], OnboardingSteps.Mark(["about"], "about"));
}
