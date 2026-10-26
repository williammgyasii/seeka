using Microsoft.EntityFrameworkCore;
using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;
using Seeka.Managers.Contracts.Resumes;
using Seeka.ResourceAccess.Accessors;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.Backend.Tests;

public sealed class ResumeAccessTests : IDisposable
{
    private static readonly ResumeDraft Draft =
        new("Ama Mensah", "Senior", 8, "Eight years in HR.", ["Recruitment"], ["HR Manager"]);

    private readonly SeekaDbContext db = new(
        new DbContextOptionsBuilder<SeekaDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private readonly Guid workspaceId = Guid.NewGuid();
    private ResumeAccess Access => new(db);

    [Fact]
    public async Task Claim_is_found_by_key_as_pending()
    {
        var claim = Claim(Guid.NewGuid());

        Assert.True(await Access.ClaimAsync(claim, default));

        var found = await Access.FindByKeyAsync(workspaceId, claim.IdempotencyKey, default);
        Assert.Equal(ResumeStatus.Pending, found!.Status);
        Assert.Equal("cv.pdf", found.FileName);
    }

    [Fact]
    public async Task Second_claim_for_the_same_key_fails()
    {
        var key = Guid.NewGuid();
        await Access.ClaimAsync(Claim(key), default);

        Assert.False(await Access.ClaimAsync(Claim(key), default));
    }

    [Fact]
    public async Task Same_key_in_another_workspace_is_independent()
    {
        var key = Guid.NewGuid();
        await Access.ClaimAsync(Claim(key), default);

        Assert.True(await Access.ClaimAsync(Claim(key) with { WorkspaceId = Guid.NewGuid() }, default));
    }

    [Fact]
    public async Task Completed_resume_keeps_its_draft_and_becomes_current()
    {
        var claim = Claim(Guid.NewGuid());
        await Access.ClaimAsync(claim, default);

        await Access.CompleteAsync(claim.Id, ResumeStatus.Extracted, Draft, default);

        var current = await Access.FindCurrentAsync(workspaceId, default);
        Assert.Equal(claim.Id, current!.Id);
        Assert.Equal(ResumeStatus.Extracted, current.Status);
        Assert.Equal("Ama Mensah", current.Draft!.FullName);
        Assert.Equal(["Recruitment"], current.Draft.Skills);
    }

    [Fact]
    public async Task Completing_a_newer_resume_replaces_the_older_one()
    {
        var older = Claim(Guid.NewGuid());
        var newer = Claim(Guid.NewGuid());
        await Access.ClaimAsync(older, default);
        await Access.CompleteAsync(older.Id, ResumeStatus.Extracted, Draft, default);
        await Access.ClaimAsync(newer, default);

        var replaced = await Access.CompleteAsync(newer.Id, ResumeStatus.Unreadable, null, default);

        Assert.Equal([older.Id], replaced);
        Assert.Equal(newer.Id, (await Access.FindCurrentAsync(workspaceId, default))!.Id);
        Assert.NotNull(await Access.FindByKeyAsync(workspaceId, older.IdempotencyKey, default));
    }

    [Fact]
    public async Task Pending_claim_is_not_current()
    {
        await Access.ClaimAsync(Claim(Guid.NewGuid()), default);

        Assert.Null(await Access.FindCurrentAsync(workspaceId, default));
    }

    [Fact]
    public async Task Released_claim_is_gone()
    {
        var claim = Claim(Guid.NewGuid());
        await Access.ClaimAsync(claim, default);

        await Access.ReleaseAsync(claim.Id, default);

        Assert.Null(await Access.FindByKeyAsync(workspaceId, claim.IdempotencyKey, default));
    }

    public void Dispose() => db.Dispose();

    private NewResume Claim(Guid key) =>
        new(Guid.NewGuid(), workspaceId, key, "cv.pdf", 1_000, DateTimeOffset.UtcNow);
}
