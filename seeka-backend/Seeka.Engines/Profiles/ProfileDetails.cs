namespace Seeka.Engines.Profiles;

public sealed record ProfileDetails(
    WorkExperience[] Experience,
    Education[] Education,
    Certification[] Certifications,
    LanguageSkill[] Languages,
    ProfileLink[] Links,
    Project[] Projects,
    Volunteering[] Volunteering)
{
    public static ProfileDetails Empty { get; } = new([], [], [], [], [], [], []);
}
