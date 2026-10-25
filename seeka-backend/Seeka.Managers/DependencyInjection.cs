using Microsoft.Extensions.DependencyInjection;
using Seeka.Managers.JobSearch;
using Seeka.Managers.Membership;
using Seeka.Managers.Outreach;

namespace Seeka.Managers;

public static class DependencyInjection
{
    public static IServiceCollection AddSeekaManagers(this IServiceCollection services) =>
        services
            .AddScoped<MembershipManager>()
            .AddScoped<JobSearchManager>()
            .AddScoped<OutreachManager>();
}
