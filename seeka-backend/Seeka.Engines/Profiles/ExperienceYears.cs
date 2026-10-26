namespace Seeka.Engines.Profiles;

public static class ExperienceYears
{
    /// <summary>
    /// Whole years covered by dated jobs, counting overlapping months once and
    /// running current roles to today. A past job with no end month is left out
    /// because its length is unknown. Null when no job can be measured.
    /// </summary>
    public static int? Total(IEnumerable<WorkExperience> experience, DateOnly today)
    {
        var now = today.Year * 12 + today.Month - 1;
        var periods = experience
            .Select(job => (
                Start: ProfileLimits.MonthIndex(job.StartMonth),
                End: job.Current ? now : ProfileLimits.MonthIndex(job.EndMonth)))
            .Where(period => period.Start is not null && period.End is not null && period.End >= period.Start)
            .Select(period => (Start: period.Start!.Value, End: period.End!.Value))
            .OrderBy(period => period.Start)
            .ToList();
        if (periods.Count == 0) return null;

        var months = 0;
        var (start, end) = periods[0];
        foreach (var period in periods.Skip(1))
        {
            if (period.Start <= end)
            {
                end = Math.Max(end, period.End);
                continue;
            }
            months += end - start;
            (start, end) = period;
        }
        months += end - start;
        return months / 12;
    }
}
