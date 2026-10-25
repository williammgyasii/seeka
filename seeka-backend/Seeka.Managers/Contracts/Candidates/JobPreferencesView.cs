namespace Seeka.Managers.Contracts.Candidates;

public sealed record JobPreferencesView(
    Guid Id,
    Guid WorkspaceId,
    string[] TargetJobTitles,
    string[] EmploymentTypes,
    string AvailableWorkingHours,
    decimal? MinimumCompensation,
    string? CompensationCurrency,
    DateTimeOffset UpdatedAt);
