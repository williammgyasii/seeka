using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record AboutStep(AboutYou About) : ProfileStep
{
    public override string Name => OnboardingSteps.About;
}
