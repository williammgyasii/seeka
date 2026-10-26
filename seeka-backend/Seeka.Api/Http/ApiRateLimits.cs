using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Seeka.Api.Http;

/// <summary>
/// In-memory limits, so each API instance counts separately. An edge rule is what
/// makes a limit hold across instances.
/// </summary>
public static class ApiRateLimits
{
    public const string Auth = "auth";
    public const int PerCallerPerMinute = 120;
    public const int AuthPerIpPerMinute = 10;
    public const string Resume = "resume";
    public const int ResumeUploadsPerHour = 5;

    public static PartitionedRateLimiter<HttpContext> CreateGlobal() =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
            FixedWindowPerMinute(CallerKey(context), PerCallerPerMinute));

    public static RateLimitPartition<string> AuthPartition(HttpContext context) =>
        FixedWindowPerMinute(IpKey(context), AuthPerIpPerMinute);

    /// <summary>Each upload is a paid AI call; this is the spend guard.</summary>
    public static RateLimitPartition<string> ResumePartition(HttpContext context) =>
        RateLimitPartition.GetFixedWindowLimiter(CallerKey(context), _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = ResumeUploadsPerHour,
            Window = TimeSpan.FromHours(1),
            QueueLimit = 0
        });

    private static string CallerKey(HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) is { } userId
            ? $"user:{userId}"
            : IpKey(context);

    private static string IpKey(HttpContext context) =>
        $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

    private static RateLimitPartition<string> FixedWindowPerMinute(string key, int permits) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permits,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
}
