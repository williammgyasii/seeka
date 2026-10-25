using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Seeka.Api.Http;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.BackgroundJobs;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.Backend.Tests;

public sealed class CloudflareRuntimeTests
{
    [Fact]
    public void Cloudflare_does_not_run_the_hangfire_server()
    {
        var configuration = new ConfigurationBuilder().Build();

        var shouldRun = BackgroundJobPolicy.ShouldRunHangfireServer(
            "Cloudflare",
            configuration);

        Assert.False(shouldRun);
    }

    [Fact]
    public void Explicit_configuration_can_disable_hangfire_elsewhere()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Hangfire:RunServer"] = "false"
            })
            .Build();

        Assert.False(BackgroundJobPolicy.ShouldRunHangfireServer(
            "Development",
            configuration));
    }

    [Theory]
    [InlineData("Cloudflare", "token", EmailDelivery.Cloudflare)]
    [InlineData("Development", "token", EmailDelivery.Cloudflare)]
    [InlineData("Development", "", EmailDelivery.Log)]
    [InlineData("Cloudflare", "", EmailDelivery.Unconfigured)]
    [InlineData("Production", null, EmailDelivery.Unconfigured)]
    public void Verification_links_are_only_logged_in_development(
        string environmentName,
        string? token,
        EmailDelivery expected)
    {
        Assert.Equal(expected, EmailDeliveryPolicy.Choose(environmentName, token));
    }

    [Fact]
    public void Proxy_configuration_uses_forwarded_scheme_and_host()
    {
        var options = HttpEdgePolicy.CreateForwardedHeadersOptions();

        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedProto));
        Assert.True(options.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedHost));
        Assert.Equal(2, options.ForwardLimit);
    }

    [Theory]
    [InlineData("POST", "cross-site", false)]
    [InlineData("DELETE", "same-origin", true)]
    [InlineData("GET", "cross-site", true)]
    public void Mutation_policy_rejects_cross_site_requests(
        string method,
        string fetchSite,
        bool expected)
    {
        Assert.Equal(expected, SameOriginRequestPolicy.IsAllowed(method, fetchSite));
    }

    [Fact]
    public void Database_context_persists_data_protection_keys()
    {
        Assert.True(typeof(Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.IDataProtectionKeyContext)
            .IsAssignableFrom(typeof(SeekaDbContext)));
        Assert.NotNull(typeof(SeekaDbContext).GetProperty("DataProtectionKeys"));
    }

    [Fact]
    public void Health_check_is_available_beneath_the_api_origin()
    {
        Assert.Equal("/api/health", SeekaApiRoutes.Health);
    }
}
