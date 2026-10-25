using Microsoft.Extensions.Logging;
using Seeka.Managers.Access.Email;

namespace Seeka.ResourceAccess.Accessors;

/// <summary>Stands in for a real provider when no email token is configured, e.g. local development.</summary>
internal sealed class LoggingEmailAccess(ILogger<LoggingEmailAccess> logger) : IEmailAccess
{
    public Task<bool> SendAsync(OutgoingEmail email, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Email not sent (no Email:CloudflareApiToken). To {To}, subject {Subject}:\n{Text}",
            email.To,
            email.Content.Subject,
            email.Content.Text);
        return Task.FromResult(true);
    }
}
