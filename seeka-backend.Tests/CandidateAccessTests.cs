using Microsoft.EntityFrameworkCore;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.Persistence;
using Seeka.ResourceAccess.Persistence.Entities;

namespace Seeka.Backend.Tests;

public sealed class CandidateAccessTests : IDisposable
{
    private readonly SeekaDbContext db = new(
        new DbContextOptionsBuilder<SeekaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Guid workspaceId = Guid.NewGuid();
    private CandidateAccess Access => new(db);

    [Theory]
    [InlineData("Senior HR Business Partner", "Senior")]
    [InlineData("People Operations Specialist", null)]
    public async Task A_profile_saved_before_fixed_levels_reads_as_a_level(string stored, string? expected)
    {
        await SeedAsync(stored);

        var profile = await Access.FindProfileAsync(workspaceId, default);

        Assert.Equal(expected, profile.About.Seniority);
    }

    [Fact]
    public async Task Matching_reads_the_same_level()
    {
        await SeedAsync("HR Director");

        var setup = await Access.FindSetupAsync(workspaceId, default);

        Assert.Equal("Director", setup!.Profile.Seniority);
    }

    private async Task SeedAsync(string seniority)
    {
        db.CandidateProfiles.Add(new CandidateProfile
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            FullName = "Ama Mensah",
            CountryOfResidence = "Ghana",
            TimeZone = "Africa/Accra",
            Seniority = seniority,
            CompletedSteps = ["about", "preferences"]
        });
        db.JobPreferences.Add(new JobPreference
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            TargetJobTitles = ["HR Manager"],
            EmploymentTypes = ["Full-time"]
        });
        await db.SaveChangesAsync();
    }

    public void Dispose() => db.Dispose();
}
