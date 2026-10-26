using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record ExperienceStep(WorkExperience[] Experience) : ProfileStep
{
    public override string Name => OnboardingSteps.Experience;
}
