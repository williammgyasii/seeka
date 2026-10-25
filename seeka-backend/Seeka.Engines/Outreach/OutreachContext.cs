namespace Seeka.Engines.Outreach;

public sealed record OutreachContext(
    string CandidateName,
    string Country,
    string TimeZone,
    string ExperienceSummary,
    string JobTitle,
    string CompanyName,
    string ContactName);
