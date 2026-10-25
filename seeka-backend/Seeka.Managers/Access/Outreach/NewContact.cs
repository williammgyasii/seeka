using Seeka.Engines.Outreach;

namespace Seeka.Managers.Access.Outreach;

public sealed record NewContact(
    string Name,
    string Title,
    string? ProfessionalProfileUrl,
    string? ProfessionalEmail,
    string EmailSource,
    ContactVerificationStatus VerificationStatus,
    string RelevanceReason,
    DateTimeOffset CreatedAt);
