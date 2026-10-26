using Seeka.Engines.Intake;
using Seeka.Engines.Resumes;

namespace Seeka.Engines.Profiles;

/// <summary>
/// Checks what a user saves for one onboarding step. Unlike résumé cleanup, it
/// never changes the input: each problem is reported against its field path,
/// such as <c>experience[1].endMonth</c>.
/// </summary>
public static class CandidateProfilePolicy
{
    public static IReadOnlyDictionary<string, string[]> CheckAbout(AboutYou about)
    {
        var errors = new FieldErrors();
        errors.Require("fullName", about.FullName, "Enter your full name.");
        errors.Require("countryOfResidence", about.CountryOfResidence, "Choose your country of residence.");
        errors.Require("timeZone", about.TimeZone, "Choose your timezone.");
        if (!string.IsNullOrWhiteSpace(about.TimeZone) && !TimeZones.IsReal(about.TimeZone))
            errors.Add("timeZone", "Choose a timezone from the list.");
        errors.MaxLength("fullName", about.FullName, ProfileLimits.MaxTextLength);
        errors.MaxLength("headline", about.Headline, ProfileLimits.MaxHeadlineLength);
        if (about.Seniority is not null && SeniorityLevel.Exact(about.Seniority) is null)
            errors.Add("seniority", "Choose a level from the list.");
        errors.MaxLength("experienceSummary", about.ExperienceSummary, ResumeDraftPolicy.MaxSummaryLength);
        if (about.YearsOfExperience is < 0 or > ResumeDraftPolicy.MaxYearsOfExperience)
            errors.Add("yearsOfExperience", $"Enter between 0 and {ResumeDraftPolicy.MaxYearsOfExperience} years.");
        errors.MaxCount("skills", about.Skills, ResumeDraftPolicy.MaxSkills);
        for (var i = 0; i < about.Skills.Length; i++)
            errors.MaxLength($"skills[{i}]", about.Skills[i], ResumeDraftPolicy.MaxItemLength);

        errors.MaxCount("languages", about.Languages, ProfileLimits.MaxLanguages);
        for (var i = 0; i < about.Languages.Length; i++)
        {
            var language = about.Languages[i];
            errors.Require($"languages[{i}].language", language.Language, "Enter the language.");
            errors.MaxLength($"languages[{i}].language", language.Language, ProfileLimits.MaxTextLength);
            if (language.Level is not null && ProfileLimits.Level(language.Level) is null)
                errors.Add($"languages[{i}].level", "Choose a level from the list.");
        }

        errors.MaxCount("links", about.Links, ProfileLimits.MaxLinks);
        for (var i = 0; i < about.Links.Length; i++)
        {
            errors.MaxLength($"links[{i}].label", about.Links[i].Label, ProfileLimits.MaxTextLength);
            errors.Https($"links[{i}].url", about.Links[i].Url, required: true);
        }

        return errors.ToDictionary();
    }

    public static IReadOnlyDictionary<string, string[]> CheckExperience(IReadOnlyList<WorkExperience> experience)
    {
        var errors = new FieldErrors();
        errors.MaxCount("experience", experience, ProfileLimits.MaxExperience);
        for (var i = 0; i < experience.Count; i++)
        {
            var job = experience[i];
            var path = $"experience[{i}]";
            errors.Require($"{path}.company", job.Company, "Enter the company.");
            errors.Require($"{path}.title", job.Title, "Enter your job title.");
            errors.MaxLength($"{path}.company", job.Company, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.title", job.Title, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.location", job.Location, ProfileLimits.MaxTextLength);
            errors.Period(path, job.StartMonth, job.EndMonth, job.Current);
            errors.MaxCount($"{path}.highlights", job.Highlights, ProfileLimits.MaxHighlights);
            for (var h = 0; h < job.Highlights.Length; h++)
                errors.MaxLength($"{path}.highlights[{h}]", job.Highlights[h], ProfileLimits.MaxHighlightLength);
        }

        return errors.ToDictionary();
    }

    public static IReadOnlyDictionary<string, string[]> CheckEducation(
        IReadOnlyList<Education> education,
        IReadOnlyList<Certification> certifications)
    {
        var errors = new FieldErrors();
        errors.MaxCount("education", education, ProfileLimits.MaxEducation);
        for (var i = 0; i < education.Count; i++)
        {
            var school = education[i];
            var path = $"education[{i}]";
            errors.Require($"{path}.school", school.School, "Enter the school.");
            errors.MaxLength($"{path}.school", school.School, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.degree", school.Degree, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.fieldOfStudy", school.FieldOfStudy, ProfileLimits.MaxTextLength);
            errors.Year($"{path}.startYear", school.StartYear);
            errors.Year($"{path}.endYear", school.EndYear);
            if (school.StartYear is { } start && school.EndYear is { } end && end < start)
                errors.Add($"{path}.endYear", "The end year can't be before the start year.");
        }

        errors.MaxCount("certifications", certifications, ProfileLimits.MaxCertifications);
        for (var i = 0; i < certifications.Count; i++)
        {
            var path = $"certifications[{i}]";
            errors.Require($"{path}.name", certifications[i].Name, "Enter the certification.");
            errors.MaxLength($"{path}.name", certifications[i].Name, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.issuer", certifications[i].Issuer, ProfileLimits.MaxTextLength);
            errors.Year($"{path}.year", certifications[i].Year);
        }

        return errors.ToDictionary();
    }

    public static IReadOnlyDictionary<string, string[]> CheckProjects(
        IReadOnlyList<Project> projects,
        IReadOnlyList<Volunteering> volunteering)
    {
        var errors = new FieldErrors();
        errors.MaxCount("projects", projects, ProfileLimits.MaxProjects);
        for (var i = 0; i < projects.Count; i++)
        {
            var path = $"projects[{i}]";
            errors.Require($"{path}.name", projects[i].Name, "Enter the project name.");
            errors.MaxLength($"{path}.name", projects[i].Name, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.description", projects[i].Description, ProfileLimits.MaxDescriptionLength);
            errors.Https($"{path}.url", projects[i].Url, required: false);
        }

        errors.MaxCount("volunteering", volunteering, ProfileLimits.MaxVolunteering);
        for (var i = 0; i < volunteering.Count; i++)
        {
            var role = volunteering[i];
            var path = $"volunteering[{i}]";
            errors.Require($"{path}.organization", role.Organization, "Enter the organization.");
            errors.MaxLength($"{path}.organization", role.Organization, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.role", role.Role, ProfileLimits.MaxTextLength);
            errors.MaxLength($"{path}.description", role.Description, ProfileLimits.MaxDescriptionLength);
            errors.Period(path, role.StartMonth, role.EndMonth, role.Current);
        }

        return errors.ToDictionary();
    }

    /// <summary>Whitespace and duplicate cleanup applied before saving; it never drops a filled-in item.</summary>
    public static AboutYou Tidy(AboutYou about) =>
        about with
        {
            FullName = about.FullName.Trim(),
            Headline = IntakeRules.OptionalText(about.Headline),
            CountryOfResidence = about.CountryOfResidence.Trim(),
            TimeZone = about.TimeZone.Trim(),
            Seniority = SeniorityLevel.Exact(about.Seniority) ?? IntakeRules.OptionalText(about.Seniority),
            ExperienceSummary = IntakeRules.OptionalText(about.ExperienceSummary),
            Skills = IntakeRules.CleanList(about.Skills),
            Languages = about.Languages
                .Select(language => new LanguageSkill(language.Language.Trim(), ProfileLimits.Level(language.Level)))
                .ToArray(),
            Links = about.Links
                .Select(link => new ProfileLink(IntakeRules.OptionalText(link.Label), link.Url.Trim()))
                .ToArray()
        };

    public static WorkExperience Tidy(WorkExperience job) =>
        job with
        {
            Company = job.Company.Trim(),
            Title = job.Title.Trim(),
            Location = IntakeRules.OptionalText(job.Location),
            StartMonth = IntakeRules.OptionalText(job.StartMonth),
            EndMonth = IntakeRules.OptionalText(job.EndMonth),
            Highlights = job.Highlights.Select(IntakeRules.OptionalText).OfType<string>().ToArray()
        };

    public static Education Tidy(Education school) =>
        school with
        {
            School = school.School.Trim(),
            Degree = IntakeRules.OptionalText(school.Degree),
            FieldOfStudy = IntakeRules.OptionalText(school.FieldOfStudy)
        };

    public static Certification Tidy(Certification certification) =>
        certification with
        {
            Name = certification.Name.Trim(),
            Issuer = IntakeRules.OptionalText(certification.Issuer)
        };

    public static Project Tidy(Project project) =>
        project with
        {
            Name = project.Name.Trim(),
            Description = IntakeRules.OptionalText(project.Description),
            Url = IntakeRules.OptionalText(project.Url)
        };

    public static Volunteering Tidy(Volunteering role) =>
        role with
        {
            Organization = role.Organization.Trim(),
            Role = IntakeRules.OptionalText(role.Role),
            StartMonth = IntakeRules.OptionalText(role.StartMonth),
            EndMonth = IntakeRules.OptionalText(role.EndMonth),
            Description = IntakeRules.OptionalText(role.Description)
        };
}
