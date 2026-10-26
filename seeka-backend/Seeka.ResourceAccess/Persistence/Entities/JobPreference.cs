using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class JobPreference
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string[] TargetJobTitles { get; set; } = [];
    public string[] EmploymentTypes { get; set; } = [];
    public decimal? MinimumCompensation { get; set; }
    public decimal? MaximumCompensation { get; set; }
    public string? CompensationCurrency { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
