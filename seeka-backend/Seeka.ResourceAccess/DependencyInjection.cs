using Hangfire.PostgreSql;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Seeka.Managers.Access.Accounts;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Jobs;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Access.Outreach;
using Seeka.Managers.Access.Pipeline;
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

        return services
            .AddScoped<IAccountAccess, AccountAccess>()
            .AddScoped<IMembershipAccess, MembershipAccess>()
            .AddScoped<ICandidateAccess, CandidateAccess>()
            .AddScoped<IJobAccess, JobAccess>()
            .AddScoped<IPipelineAccess, PipelineAccess>()
            .AddScoped<IOutreachAccess, OutreachAccess>();
    }

    public static async Task MigrateSeekaDatabaseAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SeekaDbContext>();
        await db.Database.MigrateAsync();
    }
}
