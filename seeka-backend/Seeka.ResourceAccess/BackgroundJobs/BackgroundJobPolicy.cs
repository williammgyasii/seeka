using Microsoft.Extensions.Configuration;

namespace Seeka.ResourceAccess.BackgroundJobs;

public static class BackgroundJobPolicy
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
}
