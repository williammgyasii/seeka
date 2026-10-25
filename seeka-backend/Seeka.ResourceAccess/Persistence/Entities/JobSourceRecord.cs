using Microsoft.AspNetCore.Identity;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class JobSourceRecord
{
    public Guid Id { get; set; }
    public Guid JobPostingId { get; set; }
    public JobPosting JobPosting { get; set; } = null!;
    public string Provider { get; set; } = "manual";
    public string? ExternalId { get; set; }
    public string SourceUrl { get; set; } = "";
    public string CanonicalUrl { get; set; } = "";
    public string OriginalPayload { get; set; } = "";
    public DateTimeOffset FirstSeenAt { get; set; }
    public DateTimeOffset LastCheckedAt { get; set; }
}
