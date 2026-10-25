namespace Seeka.Engines.Outreach;

public static class OutreachEngine
{
    public static IReadOnlyList<GeneratedDraft> Generate(OutreachContext context)
    {
        var experience = context.ExperienceSummary.TrimEnd(' ', '.', '!', '?');
        var greeting = $"Hi {context.ContactName},";
        var relevance =
            $"I’m {context.CandidateName}, a {context.Country}-based professional with " +
            $"{experience}. I’m interested in the {context.JobTitle} role at " +
            $"{context.CompanyName}.";
        var location =
            $"I work in {context.TimeZone} and can confirm working-hour overlap for the role.";
        var nextStep = "Would you be open to a brief conversation about what the team needs?";

        return
        [
            new(
                "email",
                $"Interest in {context.JobTitle} at {context.CompanyName}",
                $"{greeting}\n\n{relevance} {location}\n\n{nextStep}\n\nBest,\n{context.CandidateName}"),
            new(
                "linkedin_connection",
                null,
                $"Hi {context.ContactName}, I’m interested in the {context.JobTitle} role at " +
                $"{context.CompanyName}. I’m based in {context.Country} with " +
                $"{experience}. I’d value connecting."),
            new(
                "linkedin_message",
                null,
                $"{greeting} {relevance} {location} {nextStep}"),
            new(
                "follow_up",
                $"Following up: {context.JobTitle}",
                $"{greeting}\n\nI’m following up on my note about the {context.JobTitle} role at " +
                $"{context.CompanyName}. I remain interested and would welcome a short conversation.\n\n" +
                $"Best,\n{context.CandidateName}")
        ];
    }
}
