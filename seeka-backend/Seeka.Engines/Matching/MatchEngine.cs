namespace Seeka.Engines.Matching;

public static class MatchEngine
{
    public static MatchResult Assess(CandidateSnapshot candidate, JobSnapshot job)
    {
        var missingSkills = job.RequiredSkills
            .Where(required => !candidate.Skills.Any(skill => SameMeaning(skill, required)))
            .ToArray();
        var matchedSkillCount = job.RequiredSkills.Count - missingSkills.Length;
        var skillsRatio = job.RequiredSkills.Count == 0
            ? 1m
            : (decimal)matchedSkillCount / job.RequiredSkills.Count;

        var roleAligned = candidate.TargetRoles.Any(role => SameMeaning(role, job.Title));
        var seniorityKnown = !string.IsNullOrWhiteSpace(job.Seniority);
        var seniorityAligned = seniorityKnown &&
            string.Equals(candidate.Seniority, job.Seniority, StringComparison.OrdinalIgnoreCase);
        var hoursKnown = !string.IsNullOrWhiteSpace(job.WorkingHours);
        var hoursAligned = hoursKnown && IsUtcCompatible(candidate.TimeZone, job.WorkingHours!);
        var compensation = AssessCompensation(candidate, job);

        var components = new[]
        {
            Component("Target role alignment", roleAligned ? 25 : 10, 25,
                roleAligned ? Compatibility.Compatible : Compatibility.PartiallyCompatible,
                roleAligned ? "The job title aligns with a target role." : "The title is adjacent to, but not an exact target role."),
            Component("Relevant experience", candidate.YearsOfExperience > 0 ? 20 : 0, 20,
                candidate.YearsOfExperience > 0 ? Compatibility.Compatible : Compatibility.Unknown,
                $"{candidate.YearsOfExperience} years of experience are recorded."),
            Component("Required skills", (int)Math.Round(skillsRatio * 25), 25,
                missingSkills.Length == 0 ? Compatibility.Compatible : Compatibility.PartiallyCompatible,
                $"{matchedSkillCount} of {job.RequiredSkills.Count} listed skills match."),
            Component("Seniority", seniorityAligned ? 10 : 0, 10,
                !seniorityKnown ? Compatibility.Unknown :
                seniorityAligned ? Compatibility.Compatible : Compatibility.Incompatible,
                !seniorityKnown ? "The posting does not disclose seniority." :
                seniorityAligned ? "Candidate and job seniority align." : "Seniority does not align."),
            Component("Working-hour compatibility", hoursAligned ? 10 : 0, 10,
                !hoursKnown ? Compatibility.Unknown :
                hoursAligned ? Compatibility.Compatible : Compatibility.Unknown,
                !hoursKnown ? "The posting does not disclose working hours." :
                hoursAligned ? "The recorded timezone is compatible with the stated hours." :
                "Working-hour compatibility requires confirmation."),
            compensation
        };

        return new MatchResult(
            "v1",
            components.Sum(component => component.EarnedPoints),
            components,
            missingSkills);
    }

    private static MatchComponent AssessCompensation(
        CandidateSnapshot candidate,
        JobSnapshot job)
    {
        if (job.MinimumCompensation is null ||
            job.MaximumCompensation is null ||
            string.IsNullOrWhiteSpace(job.CompensationCurrency))
        {
            return Component(
                "Compensation compatibility",
                0,
                10,
                Compatibility.Unknown,
                "Compensation was not fully disclosed.");
        }

        if (candidate.MinimumCompensation is null ||
            string.IsNullOrWhiteSpace(candidate.CompensationCurrency) ||
            !string.Equals(
                candidate.CompensationCurrency,
                job.CompensationCurrency,
                StringComparison.OrdinalIgnoreCase))
        {
            return Component(
                "Compensation compatibility",
                0,
                10,
                Compatibility.Unknown,
                "Compensation cannot be compared in the same recorded currency.");
        }

        var compatible = job.MaximumCompensation >= candidate.MinimumCompensation;
        return Component(
            "Compensation compatibility",
            compatible ? 10 : 0,
            10,
            compatible ? Compatibility.Compatible : Compatibility.Incompatible,
            compatible
                ? "The disclosed range can meet the candidate minimum."
                : "The disclosed maximum is below the candidate minimum.");
    }

    private static MatchComponent Component(
        string name,
        int earned,
        int available,
        Compatibility compatibility,
        string explanation) =>
        new(name, earned, available, compatibility, explanation);

    private static bool SameMeaning(string left, string right)
    {
        var normalizedLeft = Normalize(left);
        var normalizedRight = Normalize(right);
        return normalizedLeft.Contains(normalizedRight, StringComparison.Ordinal) ||
               normalizedRight.Contains(normalizedLeft, StringComparison.Ordinal);
    }

    private static string Normalize(string value) =>
        string.Join(
            ' ',
            value.ToLowerInvariant()
                .Split([' ', '-', '/', ','], StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word is not "senior" and not "junior" and not "remote"));

    private static bool IsUtcCompatible(string timeZone, string workingHours) =>
        string.Equals(timeZone, "Africa/Accra", StringComparison.OrdinalIgnoreCase) &&
        workingHours.Contains("UTC", StringComparison.OrdinalIgnoreCase);
}
