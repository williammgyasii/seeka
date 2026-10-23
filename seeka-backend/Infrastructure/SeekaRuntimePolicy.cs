using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;

namespace Seeka.Infrastructure;

public static class SeekaRuntimePolicy
{
    public static bool ShouldRunHangfireServer(
        string environmentName,
        IConfiguration configuration)
    {
        var configured = configuration.GetValue<bool?>("Hangfire:RunServer");
        return configured ?? !string.Equals(
            environmentName,
            "Cloudflare",
            StringComparison.OrdinalIgnoreCase);
    }

    public static ForwardedHeadersOptions CreateForwardedHeadersOptions()
    {
        var options = new ForwardedHeadersOptions
        {
            ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedHost |
                ForwardedHeaders.XForwardedProto,
            ForwardLimit = 2
        };

        // The container has no public route; requests arrive through its Worker.
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
        return options;
    }
}

public static class SameOriginRequestPolicy
{
    public static bool IsAllowed(string method, string? fetchSite)
    {
        if (HttpMethods.IsGet(method) ||
            HttpMethods.IsHead(method) ||
            HttpMethods.IsOptions(method) ||
            HttpMethods.IsTrace(method))
        {
            return true;
        }

        return string.IsNullOrWhiteSpace(fetchSite) ||
               string.Equals(fetchSite, "same-origin", StringComparison.OrdinalIgnoreCase);
    }
}

public static class SeekaApiRoutes
{
    public const string Health = "/api/health";
}
