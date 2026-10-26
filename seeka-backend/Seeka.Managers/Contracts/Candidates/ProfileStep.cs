using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

/// <summary>What one onboarding step saves. Each replaces that step's data whole.</summary>
public abstract record ProfileStep
{
    public abstract string Name { get; }
}
