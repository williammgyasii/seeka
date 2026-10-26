using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Seeka.Api.Http;

namespace Seeka.Backend.Tests;

public sealed class ApiRateLimitsTests
{
    [Fact]
    public void A_signed_in_user_is_limited_per_user_and_others_are_unaffected()
    {
        using var limiter = ApiRateLimits.CreateGlobal();
        var ama = SignedIn("ama", "10.0.0.1");

        for (var i = 0; i < ApiRateLimits.PerCallerPerMinute; i++)
            Assert.True(limiter.AttemptAcquire(ama).IsAcquired);

        Assert.False(limiter.AttemptAcquire(ama).IsAcquired);
        Assert.True(limiter.AttemptAcquire(SignedIn("kofi", "10.0.0.1")).IsAcquired);
    }

    [Fact]
    public void Signed_out_callers_are_limited_per_IP()
    {
        using var limiter = ApiRateLimits.CreateGlobal();

        for (var i = 0; i < ApiRateLimits.PerCallerPerMinute; i++)
            Assert.True(limiter.AttemptAcquire(Anonymous("10.0.0.2")).IsAcquired);

        Assert.False(limiter.AttemptAcquire(Anonymous("10.0.0.2")).IsAcquired);
        Assert.True(limiter.AttemptAcquire(Anonymous("10.0.0.3")).IsAcquired);
    }

    [Fact]
    public void Resume_uploads_are_limited_per_user_per_hour()
    {
        var ama = ApiRateLimits.ResumePartition(SignedIn("ama", "10.0.0.1"));
        using var limiter = ama.Factory(ama.PartitionKey);

        for (var i = 0; i < ApiRateLimits.ResumeUploadsPerHour; i++)
            Assert.True(limiter.AttemptAcquire().IsAcquired);

        Assert.False(limiter.AttemptAcquire().IsAcquired);
        Assert.Equal("user:ama", ama.PartitionKey);
    }

    private static DefaultHttpContext SignedIn(string userId, string ip)
    {
        var context = Anonymous(ip);
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId)],
            "test"));
        return context;
    }

    private static DefaultHttpContext Anonymous(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return context;
    }
}
