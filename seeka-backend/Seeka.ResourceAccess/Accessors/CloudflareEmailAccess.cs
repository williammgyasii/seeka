using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Seeka.Managers.Access.Email;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class CloudflareEmailAccess(
    HttpClient http,
    CloudflareEmailSettings settings,
    ILogger<CloudflareEmailAccess> logger) : IEmailAccess
{
    public async Task<bool> SendAsync(OutgoingEmail email, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.cloudflare.com/client/v4/accounts/{settings.AccountId}/email/sending/send")
        {
            Content = JsonContent.Create(new
            {
                to = email.To,
                from = new { address = settings.FromAddress, name = settings.FromName },
                subject = email.Content.Subject,
                text = email.Content.Text,
                html = email.Content.Html
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiToken);
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode) return true;
            logger.LogError(
                "Cloudflare Email Sending rejected an email with status {Status}: {Body}",
                (int)response.StatusCode,
                await response.Content.ReadAsStringAsync(cancellationToken));
            return false;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Cloudflare Email Sending was unreachable.");
            return false;
        }
    }
}
