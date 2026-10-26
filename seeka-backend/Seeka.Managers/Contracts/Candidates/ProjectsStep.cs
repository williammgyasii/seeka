using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record ProjectsStep(Project[] Projects, Volunteering[] Volunteering) : ProfileStep
{
    public override string Name => OnboardingSteps.Projects;
}
