namespace Seeka.Managers.Contracts.Outreach;

public sealed record AddContactRequest(
    string Name,
    string Title,
    string? ProfessionalProfileUrl,
    string? ProfessionalEmail,
    string RelevanceReason);
