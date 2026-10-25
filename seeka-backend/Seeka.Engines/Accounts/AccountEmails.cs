using System.Net;

namespace Seeka.Engines.Accounts;

public static class AccountEmails
{
    public static string VerificationLink(string publicUrl, Guid userId, string token) =>
        $"{publicUrl.TrimEnd('/')}/verify-email/confirm?userId={userId}&token={Uri.EscapeDataString(token)}";

    public static EmailContent Verification(string displayName, string link)
    {
        var name = WebUtility.HtmlEncode(displayName);
        var href = WebUtility.HtmlEncode(link);
        var text = $"""
            Hi {displayName},

            Confirm your email address to start using Seeka:
            {link}

            This link expires in 24 hours. If you didn't create a Seeka account, ignore this email.
            """;
        var html = $"""
            <div style="font-family:system-ui,sans-serif;max-width:480px;margin:0 auto;color:#171f3e">
              <p style="font-size:18px;font-weight:600">Hi {name},</p>
              <p>Confirm your email address to start using Seeka.</p>
              <p><a href="{href}" style="display:inline-block;background:#1b2657;color:#ffffff;padding:12px 20px;border-radius:8px;text-decoration:none;font-weight:600">Confirm email</a></p>
              <p style="font-size:13px;color:#5b6178">This link expires in 24 hours. If you didn't create a Seeka account, ignore this email.</p>
            </div>
            """;
        return new EmailContent("Confirm your email for Seeka", text, html);
    }
}
