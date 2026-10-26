namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class Resume
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid IdempotencyKey { get; set; }
    public string FileName { get; set; } = "";
    public long SizeBytes { get; set; }
    public string Status { get; set; } = "";
    public string? DraftJson { get; set; }
    public bool IsCurrent { get; set; }
    public DateTimeOffset ClaimedAt { get; set; }
}
