using Seeka.Engines.Outreach;

namespace Seeka.Managers.Contracts.Outreach;

public sealed record ContactView(
    Guid Id,
    Guid WorkspaceId,
    Guid CompanyId,
    string Name,
    string Title,
    string? ProfessionalProfileUrl,
    string? ProfessionalEmail,
    string? EmailSource,
    ContactVerificationStatus VerificationStatus,
    string RelevanceReason,
    DateTimeOffset CreatedAt);
