using Microsoft.AspNetCore.HttpOverrides;

namespace Seeka.Api.Http;

public static class HttpEdgePolicy
{
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
