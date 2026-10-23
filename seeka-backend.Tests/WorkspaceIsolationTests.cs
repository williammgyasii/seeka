using Microsoft.EntityFrameworkCore;
using Seeka.Application.Jobs;
using Seeka.Infrastructure;
using Seeka.Infrastructure.Entities;

namespace Seeka.Backend.Tests;

public sealed class WorkspaceIsolationTests
{
    [Fact]
    public async Task Saved_job_reader_does_not_return_another_workspaces_record()
    {
        var options = new DbContextOptionsBuilder<SeekaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var db = new SeekaDbContext(options);

        var ownerWorkspaceId = Guid.NewGuid();
        var otherWorkspaceId = Guid.NewGuid();
        var job = new JobPosting
        {
            Id = Guid.NewGuid(),
            CompanyName = "Acme",
            Title = "Administrative Assistant",
            Description = "Support a distributed operations team.",
            OriginalUrl = "https://example.com/jobs/admin",
            CanonicalUrl = "https://example.com/jobs/admin",
            FirstSeenAt = DateTimeOffset.UtcNow,
            LastCheckedAt = DateTimeOffset.UtcNow
        };
        var savedJob = new SavedJob
        {
            Id = Guid.NewGuid(),
            WorkspaceId = ownerWorkspaceId,
            JobPostingId = job.Id,
            Stage = PipelineStage.Saved,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.JobPostings.Add(job);
        db.SavedJobs.Add(savedJob);
        await db.SaveChangesAsync();

        var reader = new WorkspaceJobReader(db);

        Assert.Null(await reader.FindAsync(otherWorkspaceId, savedJob.Id));
        Assert.NotNull(await reader.FindAsync(ownerWorkspaceId, savedJob.Id));
    }
}
