using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record EducationStep(Education[] Education, Certification[] Certifications) : ProfileStep
{
    public override string Name => OnboardingSteps.Education;
}
