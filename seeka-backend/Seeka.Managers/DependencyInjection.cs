using Microsoft.Extensions.DependencyInjection;
using Seeka.Managers.Candidates;
using Seeka.Managers.JobSearch;
using Seeka.Managers.Membership;
using Seeka.Managers.Outreach;
using Seeka.Managers.Resumes;

namespace Seeka.Managers;

public static class DependencyInjection
{
    public static IServiceCollection AddSeekaManagers(this IServiceCollection services) =>
        services
            .AddScoped<MembershipManager>()
            .AddScoped<CandidateProfileManager>()
            .AddScoped<JobSearchManager>()
            .AddScoped<OutreachManager>()
            .AddScoped<ResumeManager>();
}
