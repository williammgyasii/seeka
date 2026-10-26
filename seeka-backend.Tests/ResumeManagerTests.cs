using System.Text;
using Seeka.Backend.Tests.Fakes;
using Seeka.Managers.Contracts.Resumes;
using Seeka.Managers.Resumes;

namespace Seeka.Backend.Tests;

public sealed class ResumeManagerTests
{
    private static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7 résumé");
    private readonly Guid userId = Guid.NewGuid();
    private readonly Guid workspaceId = Guid.NewGuid();
    private readonly FakeResumeAccess resumes = new();
    private readonly FakeResumeFileAccess files = new();
    private readonly FakePdfTextAccess pdfs = new();
    private readonly FakeResumeExtractionAccess extractor = new();

    [Fact]
    public async Task Readable_resume_returns_a_cleaned_draft()
    {
        var outcome = await Upload(Guid.NewGuid());

        Assert.Equal(ResumeStatus.Extracted, outcome.Resume!.Status);
        Assert.Equal("Ama Mensah", outcome.Resume.Draft!.FullName);
        Assert.Equal(["Recruitment"], outcome.Resume.Draft.Skills);
        Assert.Equal("cv.pdf", outcome.Resume.FileName);
        Assert.Single(files.Files);
    }

    [Fact]
    public async Task Rejected_upload_stores_nothing()
    {
        var outcome = await NewManager().UploadAsync(
            userId, new ResumeUpload(Guid.NewGuid(), "cv.docx", Encoding.ASCII.GetBytes("PK..")), default);

        Assert.Equal("file", outcome.Rejection!.Field);
        Assert.Empty(resumes.Rows);
        Assert.Empty(files.Files);
    }

    [Fact]
    public async Task Pdf_without_text_is_kept_but_not_extracted()
    {
        pdfs.Text = "   ";

        var outcome = await Upload(Guid.NewGuid());

        Assert.Equal(ResumeStatus.Unreadable, outcome.Resume!.Status);
        Assert.Null(outcome.Resume.Draft);
        Assert.Equal(0, extractor.Calls);
        Assert.Single(files.Files);
    }

    [Fact]
    public async Task Extractor_failure_keeps_the_file()
    {
        extractor.Facts = null;

        var outcome = await Upload(Guid.NewGuid());

        Assert.Equal(ResumeStatus.ExtractionFailed, outcome.Resume!.Status);
        Assert.Single(files.Files);
    }

    [Fact]
    public async Task Repeated_key_replays_the_first_result()
    {
        var key = Guid.NewGuid();
        var first = await Upload(key);

        var second = await Upload(key);

        Assert.Equal(first.Resume, second.Resume);
        Assert.Equal(1, extractor.Calls);
        Assert.Single(files.Files);
    }

    [Fact]
    public async Task Key_still_running_reports_in_progress()
    {
        var key = Guid.NewGuid();
        resumes.SeedPending(workspaceId, key, DateTimeOffset.UtcNow);

        var outcome = await Upload(key);

        Assert.True(outcome.InProgress);
        Assert.Equal(0, extractor.Calls);
    }

    [Fact]
    public async Task Abandoned_claim_is_retaken()
    {
        var key = Guid.NewGuid();
        resumes.SeedPending(workspaceId, key, DateTimeOffset.UtcNow.AddMinutes(-10));

        var outcome = await Upload(key);

        Assert.Equal(ResumeStatus.Extracted, outcome.Resume!.Status);
        Assert.Single(resumes.Rows);
    }

    [Fact]
    public async Task Storage_failure_releases_the_claim()
    {
        files.FailSaves = true;
        var key = Guid.NewGuid();

        await Assert.ThrowsAsync<IOException>(() => Upload(key));

        Assert.Empty(resumes.Rows);
    }

    [Fact]
    public async Task Cancelled_extraction_releases_the_claim_and_the_file()
    {
        extractor.Throw = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => Upload(Guid.NewGuid()));

        Assert.Empty(resumes.Rows);
        Assert.Empty(files.Files);
    }

    [Fact]
    public async Task New_upload_replaces_and_deletes_the_old_file()
    {
        await Upload(Guid.NewGuid());
        var oldFile = files.Files.Keys.Single();

        await Upload(Guid.NewGuid());

        Assert.DoesNotContain(oldFile, files.Files.Keys);
        Assert.Single(files.Files);
    }

    [Fact]
    public async Task Current_resume_returns_the_stored_draft()
    {
        await Upload(Guid.NewGuid());

        var current = await NewManager().FindCurrentAsync(userId, default);

        Assert.Equal("Ama Mensah", current!.Draft!.FullName);
        Assert.Equal(1, extractor.Calls);
    }

    [Fact]
    public async Task Current_file_opens_the_stored_pdf()
    {
        await Upload(Guid.NewGuid());

        var file = await NewManager().OpenCurrentFileAsync(userId, default);

        Assert.Equal("cv.pdf", file!.FileName);
        using var reader = new StreamReader(file.Content);
        Assert.StartsWith("%PDF-", await reader.ReadToEndAsync());
    }

    private Task<ResumeUploadOutcome> Upload(Guid key) =>
        NewManager().UploadAsync(userId, new ResumeUpload(key, "cv.pdf", Pdf), default);

    private ResumeManager NewManager() =>
        new(new FakeMembershipAccess(workspaceId), resumes, files, pdfs, extractor, TimeProvider.System);
}
