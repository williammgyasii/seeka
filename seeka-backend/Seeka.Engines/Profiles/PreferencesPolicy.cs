using Seeka.Engines.Intake;
using Seeka.Engines.Resumes;

namespace Seeka.Engines.Profiles;

/// <summary>Checks and tidies the preferences step against the fixed lists it is chosen from.</summary>
public static class PreferencesPolicy
{
    public static readonly IReadOnlyList<string> EmploymentTypes =
        ["Full-time", "Part-time", "Contract", "Freelance", "Internship", "Temporary"];

    public static IReadOnlyDictionary<string, string[]> Check(SearchPreferences preferences, JobTitleCatalog titles)
    {
        var errors = new FieldErrors();
        if (preferences.TargetJobTitles.Length == 0)
            errors.Add("targetJobTitles", "Add at least one role you are looking for.");
        errors.MaxCount("targetJobTitles", preferences.TargetJobTitles, ResumeDraftPolicy.MaxTargetJobTitles);
        for (var i = 0; i < preferences.TargetJobTitles.Length; i++)
            if (titles.Preferred(preferences.TargetJobTitles[i]) is null)
                errors.Add($"targetJobTitles[{i}]", "Choose a title from the list.");

        for (var i = 0; i < preferences.EmploymentTypes.Length; i++)
            if (EmploymentType(preferences.EmploymentTypes[i]) is null)
                errors.Add($"employmentTypes[{i}]", "Choose a type from the list.");

        CheckPay(errors, preferences);
        return errors.ToDictionary();
    }

    /// <summary>Stores titles and types by their list names; an unknown value is kept so the check can name it.</summary>
    public static SearchPreferences Tidy(SearchPreferences preferences, JobTitleCatalog titles) =>
        preferences with
        {
            TargetJobTitles = IntakeRules.CleanList(
                preferences.TargetJobTitles.Select(title => titles.Preferred(title) ?? title)),
            EmploymentTypes = IntakeRules.CleanList(
                preferences.EmploymentTypes.Select(type => EmploymentType(type) ?? type)),
            CompensationCurrency = IntakeRules.Currency(preferences.CompensationCurrency)
        };

    private static void CheckPay(FieldErrors errors, SearchPreferences preferences)
    {
        var (minimum, maximum, currency) =
            (preferences.MinimumCompensation, preferences.MaximumCompensation, preferences.CompensationCurrency);
        var payChosen = minimum is not null || maximum is not null;

        if (string.IsNullOrWhiteSpace(currency))
        {
            if (payChosen) errors.Add("compensationCurrency", "Choose a currency for the pay.");
        }
        else if (!PayBands.HasBands(currency))
        {
            errors.Add("compensationCurrency", "Choose a currency from the list.");
        }
        else if (payChosen && !PayBands.IsBand(currency, minimum, maximum))
        {
            errors.Add("minimumCompensation", "Choose a pay band from the list.");
        }
    }

    private static string? EmploymentType(string value) =>
        EmploymentTypes.FirstOrDefault(type => string.Equals(type, value.Trim(), StringComparison.OrdinalIgnoreCase));
}
