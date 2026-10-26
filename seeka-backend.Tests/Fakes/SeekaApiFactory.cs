using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Access.Email;
using Seeka.Managers.Access.Resumes;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class SeekaApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();

    public FakeEmailAccess Email { get; } = new();
    public FakeResumeFileAccess ResumeFiles { get; } = new();
    public FakeResumeExtractionAccess Extractor { get; } = new();

    public HttpClient CreateBrowser() =>
        CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true,
            AllowAutoRedirect = false
        });

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:Seeka", "Host=unused;Database=unused");
        builder.UseSetting("Hangfire:RunServer", "false");
        builder.UseSetting("App:PublicUrl", "https://app.test");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<SeekaDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SeekaDbContext>>();
            services.AddDbContext<SeekaDbContext>(options => options
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            services.RemoveAll<IEmailAccess>();
            services.AddSingleton<IEmailAccess>(Email);
            services.RemoveAll<IResumeFileAccess>();
            services.AddSingleton<IResumeFileAccess>(ResumeFiles);
            services.RemoveAll<IPdfTextAccess>();
            services.AddSingleton<IPdfTextAccess>(new FakePdfTextAccess());
            services.RemoveAll<IResumeExtractionAccess>();
            services.AddSingleton<IResumeExtractionAccess>(Extractor);
            services.RemoveAll<IJobTitleAccess>();
            services.AddSingleton<IJobTitleAccess>(new FakeJobTitleAccess());
        });
    }
}
