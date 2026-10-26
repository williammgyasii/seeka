namespace Seeka.Engines.Profiles;

public static class OnboardingSteps
{
    public const string About = "about";
    public const string Experience = "experience";
    public const string Education = "education";
    public const string Projects = "projects";
    public const string Preferences = "preferences";

    /// <summary>The order onboarding shows the steps in.</summary>
    public static readonly string[] All = [About, Experience, Education, Projects, Preferences];

    /// <summary>Matching needs who the candidate is and a target role.</summary>
    public static readonly string[] Required = [About, Preferences];

    public static string[] Missing(IEnumerable<string> completed) =>
        Required.Except(completed).ToArray();

    public static string[] Mark(IEnumerable<string> completed, string step) =>
        All.Where(completed.Append(step).Contains).ToArray();
}
