using Microsoft.AspNetCore.HttpOverrides;

namespace Seeka.Api.Http;

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
