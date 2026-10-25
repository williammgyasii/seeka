using Microsoft.Extensions.Logging.Abstractions;
using Seeka.Backend.Tests.Fakes;
using Seeka.Engines.Eligibility;
using Seeka.Engines.Matching;
using Seeka.Managers.Access.Jobs;
using Seeka.Managers.Contracts.Jobs;
using Seeka.Managers.JobSearch;

namespace Seeka.Backend.Tests;

public sealed class JobSearchManagerTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid WorkspaceId = Guid.NewGuid();

    [Fact]
    public async Task Non_http_source_is_rejected_before_any_access_call()
    {
        var jobs = new FakeJobAccess();
        var manager = Create(jobs);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            manager.ImportAsync(UserId, Request("ftp://example.com/job"), default));

        Assert.Null(jobs.Saved);
        Assert.Null(jobs.LookedUpUrl);
    }

    [Fact]
    public async Task Missing_candidate_setup_is_a_conflict()
    {
        var manager = Create(new FakeJobAccess(), hasSetup: false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            manager.ImportAsync(UserId, Request(), default));
    }

    [Fact]
    public async Task User_without_a_workspace_is_unauthorized()
    {
        var jobs = new FakeJobAccess();
        var manager = Create(jobs, hasWorkspace: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            manager.ImportAsync(UserId, Request(), default));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            manager.GetPipelineAsync(UserId, default));

        Assert.Null(jobs.LookedUpUrl);
    }

    [Fact]
    public async Task Pipeline_is_read_from_the_users_own_workspace()
    {
        var pipeline = new FakePipelineAccess();
        var manager = Create(new FakeJobAccess(), pipeline: pipeline);

        await manager.GetPipelineAsync(UserId, default);

        Assert.Equal(WorkspaceId, pipeline.ListedWorkspaceId);
    }

    [Fact]
    public async Task New_job_is_assessed_and_saved_in_one_import()
    {
        var jobs = new FakeJobAccess();
        var manager = Create(jobs);

        await manager.ImportAsync(UserId, Request("https://example.com/jobs/1?utm=x#apply"), default);

        var saved = Assert.IsType<JobImport>(jobs.Saved);
        Assert.Equal(WorkspaceId, saved.WorkspaceId);
        Assert.Equal("https://example.com/jobs/1", saved.Posting.CanonicalUrl);
        Assert.Equal(EligibilityStatus.Eligible, saved.Eligibility.Status);
        Assert.Equal("v1", saved.Match.ModelVersion);
        Assert.Equal(["Salesforce"], saved.Match.MissingRequirements);
        Assert.Equal(UserId, saved.Activity.ActorUserId);
        Assert.Equal("Imported Executive Assistant at Acme.", saved.Activity.Description);
    }

    [Fact]
    public async Task Existing_posting_is_matched_on_its_stored_fields()
    {
        var jobs = new FakeJobAccess
        {
            Target = new ImportTarget(
                new ExistingJob(
                    "Acme Corp",
                    new JobSnapshot("Office Manager", ["Excel"], null, null, null, null, null)),
                "Acme Corp")
        };
        var manager = Create(jobs);

        await manager.ImportAsync(UserId, Request(), default);

        var saved = Assert.IsType<JobImport>(jobs.Saved);
        Assert.Equal(["Excel"], saved.Match.MissingRequirements);
        Assert.Equal("Imported Office Manager at Acme Corp.", saved.Activity.Description);
    }

    private static JobSearchManager Create(
        FakeJobAccess jobs,
        bool hasWorkspace = true,
        bool hasSetup = true,
        FakePipelineAccess? pipeline = null) =>
        new(
            new FakeMembershipAccess(hasWorkspace ? WorkspaceId : null),
            new FakeCandidateAccess(hasSetup ? TestData.GhanaSetup(WorkspaceId) : null),
            jobs,
            pipeline ?? new FakePipelineAccess(),
            TimeProvider.System,
            NullLogger<JobSearchManager>.Instance);

    private static ImportJobRequest Request(string sourceUrl = "https://example.com/jobs/1") =>
        new(
            "Acme",
            null,
            "Executive Assistant",
            "This remote role is open to candidates based in Ghana.",
            sourceUrl,
            null,
            null,
            null,
            ["calendar management", "Salesforce"],
            null,
            null,
            null,
            null,
            null,
            null);
}
