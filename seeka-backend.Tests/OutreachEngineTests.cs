using Seeka.Engines.Outreach;

namespace Seeka.Backend.Tests;

public sealed class OutreachEngineTests
{
    [Fact]
    public void Drafts_are_grounded_in_saved_candidate_job_and_contact_facts()
    {
        var drafts = OutreachEngine.Generate(new OutreachContext(
            "Ama Mensah",
            "Ghana",
            "Africa/Accra",
            "5 years supporting executives and HR teams.",
            "Administrative Assistant",
            "Acme",
            "Nana Owusu"));

        Assert.Equal(4, drafts.Count);
        Assert.All(drafts, draft =>
        {
            Assert.Contains("Administrative Assistant", draft.Body);
            Assert.DoesNotContain("increased", draft.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("award", draft.Body, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("..", draft.Body, StringComparison.Ordinal);
        });
        Assert.Contains(drafts, draft => draft.Channel == "email" && draft.Subject is not null);
        Assert.Contains(drafts, draft => draft.Body.Contains("Ghana"));
    }
}
