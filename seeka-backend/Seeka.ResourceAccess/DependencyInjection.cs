using Hangfire.PostgreSql;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Email;
using Seeka.Managers.Access.Jobs;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Access.Outreach;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Access.Resumes;
using Seeka.Managers;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.BackgroundJobs;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess;

public static class DependencyInjection
{
    public static IServiceCollection AddSeekaResourceAccess(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("Seeka")
            ?? throw new InvalidOperationException("ConnectionStrings:Seeka is required.");

        services.AddDbContext<SeekaDbContext>(options => options.UseNpgsql(connectionString));
        services.AddDataProtection()
            .SetApplicationName("Seeka")
            .PersistKeysToDbContext<SeekaDbContext>();
        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 10;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<SeekaDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddHangfire(hangfire => hangfire
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString)));
        if (BackgroundJobPolicy.ShouldRunHangfireServer(environment.EnvironmentName, configuration))
            services.AddHangfireServer();

        var emailToken = configuration["Email:CloudflareApiToken"];
        switch (EmailDeliveryPolicy.Choose(environment.EnvironmentName, emailToken))
        {
            case EmailDelivery.Cloudflare:
                services.AddSingleton(new CloudflareEmailSettings(
                    configuration["Email:CloudflareAccountId"]
                        ?? throw new InvalidOperationException("Email:CloudflareAccountId is required with a token."),
                    emailToken!,
                    configuration["Email:FromAddress"] ?? "verify@seekajobs.tech",
                    configuration["Email:FromName"] ?? "Seeka"));
                services.AddHttpClient<IEmailAccess, CloudflareEmailAccess>(client =>
                    client.Timeout = TimeSpan.FromSeconds(10));
                break;
            case EmailDelivery.Log:
                services.AddScoped<IEmailAccess, LoggingEmailAccess>();
                break;
            default:
                services.AddScoped<IEmailAccess>(_ => throw new InvalidOperationException(
                    "Email:CloudflareApiToken is required outside Development."));
                break;
        }

        AddResumeAccess(services, configuration, environment);

        return services
            .AddScoped<IAccountAccess, AccountAccess>()
            .AddScoped<IMembershipAccess, MembershipAccess>()
            .AddScoped<ICandidateAccess, CandidateAccess>()
            .AddSingleton<IJobTitleAccess, EscoJobTitleAccess>()
            .AddScoped<IJobAccess, JobAccess>()
            .AddScoped<IPipelineAccess, PipelineAccess>()
            .AddScoped<IOutreachAccess, OutreachAccess>();
    }

    private static void AddResumeAccess(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddScoped<IResumeAccess, ResumeAccess>();
        services.AddSingleton<IPdfTextAccess, PdfTextAccess>();

        if (configuration["OpenAI:ApiKey"] is { Length: > 0 } openAiKey)
        {
            services.AddSingleton(new OpenAiSettings(openAiKey, configuration["OpenAI:Model"] ?? "gpt-5.4-mini"));
            services.AddHttpClient<IResumeExtractionAccess, OpenAiResumeExtractionAccess>(client =>
                client.Timeout = TimeSpan.FromSeconds(60));
        }
        else
        {
            services.AddScoped<IResumeExtractionAccess, MissingResumeExtractionAccess>();
        }

        var accessKeyId = configuration["Storage:R2AccessKeyId"];
        switch (ResumeStoragePolicy.Choose(environment.EnvironmentName, accessKeyId))
        {
            case ResumeStorage.R2:
                var settings = new R2Settings(
                    configuration["Storage:R2AccountId"]
                        ?? throw new InvalidOperationException("Storage:R2AccountId is required with R2 keys."),
                    accessKeyId!,
                    configuration["Storage:R2SecretAccessKey"]
                        ?? throw new InvalidOperationException("Storage:R2SecretAccessKey is required with R2 keys."),
                    configuration["Storage:Bucket"] ?? "seeka-resumes");
                services.AddSingleton(settings);
                services.AddSingleton(R2ResumeFileAccess.CreateClient(settings));
                services.AddSingleton<IResumeFileAccess, R2ResumeFileAccess>();
                break;
            case ResumeStorage.Local:
                var root = configuration["Storage:LocalPath"]
                    ?? Path.Combine(environment.ContentRootPath, ".data");
                services.AddSingleton<IResumeFileAccess>(provider => new LocalResumeFileAccess(
                    root,
                    provider.GetRequiredService<ILogger<LocalResumeFileAccess>>()));
                break;
            default:
                services.AddScoped<IResumeFileAccess>(_ => throw new InvalidOperationException(
                    "Storage:R2AccessKeyId is required outside Development."));
                break;
        }
    }

    public static async Task MigrateSeekaDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SeekaDbContext>();
        await db.Database.MigrateAsync();
    }
}
