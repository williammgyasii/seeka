using Microsoft.AspNetCore.Identity;
using Seeka.Engines.Outreach;

namespace Seeka.ResourceAccess.Persistence.Entities;

public sealed class Contact
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string? ProfessionalProfileUrl { get; set; }
    public string? ProfessionalEmail { get; set; }
    public string? EmailSource { get; set; }
    public ContactVerificationStatus VerificationStatus { get; set; }
    public string RelevanceReason { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
