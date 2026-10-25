using Seeka.Engines.Accounts;

namespace Seeka.Backend.Tests;

public sealed class AccountEmailsTests
{
    private const string Link = "https://seekajobs.tech/verify-email/confirm?userId=u1&token=a%2Bb";

    [Fact]
    public void Verification_email_names_the_person_and_has_a_subject()
    {
        var email = AccountEmails.Verification("Esi Owusu", Link);

        Assert.Equal("Confirm your email for Seeka", email.Subject);
        Assert.Contains("Esi Owusu", email.Text);
        Assert.Contains("Esi Owusu", email.Html);
    }

    [Fact]
    public void Verification_email_carries_the_link_in_text_and_html()
    {
        var email = AccountEmails.Verification("Esi Owusu", Link);

        Assert.Contains(Link, email.Text);
        Assert.Contains("href=\"https://seekajobs.tech/verify-email/confirm?userId=u1&amp;token=a%2Bb\"", email.Html);
    }

    [Fact]
    public void Verification_email_says_when_the_link_expires()
    {
        var email = AccountEmails.Verification("Esi Owusu", Link);

        Assert.Contains("24 hours", email.Text);
        Assert.Contains("24 hours", email.Html);
    }

    [Fact]
    public void Verification_link_points_at_the_confirm_page_with_an_escaped_token()
    {
        var userId = Guid.Parse("6f1c2d3e-0000-4000-8000-000000000001");

        var link = AccountEmails.VerificationLink("https://seekajobs.tech/", userId, "a+b/c=");

        Assert.Equal(
            "https://seekajobs.tech/verify-email/confirm?userId=6f1c2d3e-0000-4000-8000-000000000001&token=a%2Bb%2Fc%3D",
            link);
    }

    [Fact]
    public void Html_encodes_the_name_so_it_cannot_inject_markup()
    {
        var email = AccountEmails.Verification("<b>Esi</b>", Link);

        Assert.DoesNotContain("<b>Esi</b>", email.Html);
        Assert.Contains("&lt;b&gt;Esi&lt;/b&gt;", email.Html);
    }
}
