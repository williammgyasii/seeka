using Seeka.Engines.Profiles;

namespace Seeka.Managers.Contracts.Candidates;

public sealed record CandidateProfileResponse(
    AboutYou About,
    WorkExperience[] Experience,
    Education[] Education,
    Certification[] Certifications,
    Project[] Projects,
    Volunteering[] Volunteering,
    SearchPreferences? Preferences,
    string[] CompletedSteps,
    bool OnboardingComplete,
    int? SuggestedYearsOfExperience);
