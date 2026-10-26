using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Membership;
using Seeka.Managers.Access.Resumes;
using Seeka.Managers.Contracts.Resumes;

namespace Seeka.Managers.Resumes;

public sealed class ResumeManager(
    IMembershipAccess memberships,
    IResumeAccess resumes,
    IResumeFileAccess files,
    IPdfTextAccess pdfs,
    IResumeExtractionAccess extractor,
    TimeProvider timeProvider)
{
    public async Task<ResumeUploadOutcome> UploadAsync(
        Guid userId,
        ResumeUpload upload,
        CancellationToken cancellationToken)
    {
        var header = upload.Content.AsSpan(0, Math.Min(upload.Content.Length, 8));
        if (ResumeUploadPolicy.Check(upload.IdempotencyKey, upload.Content.Length, header) is { } rejection)
            return ResumeUploadOutcome.Rejected(rejection);

        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var key = upload.IdempotencyKey!.Value;
        var now = timeProvider.GetUtcNow();

        var existing = await resumes.FindByKeyAsync(workspace.WorkspaceId, key, cancellationToken);
        if (existing is not null)
        {
            if (existing.Status != ResumeStatus.Pending) return ResumeUploadOutcome.Completed(View(existing));
            if (!ResumeUploadPolicy.IsAbandoned(existing.ClaimedAt, now)) return ResumeUploadOutcome.Running();
            await resumes.ReleaseAsync(existing.Id, cancellationToken);
        }

        var claim = new NewResume(
            Guid.NewGuid(),
            workspace.WorkspaceId,
            key,
            ResumeUploadPolicy.DisplayName(upload.FileName),
            upload.Content.Length,
            now);
        if (!await resumes.ClaimAsync(claim, cancellationToken)) return ResumeUploadOutcome.Running();

        ResumeStatus status;
        ResumeDraft? draft;
        IReadOnlyList<Guid> replaced;
        try
        {
            await files.SaveAsync(claim.WorkspaceId, claim.Id, upload.Content, cancellationToken);
            (status, draft) = await ExtractAsync(upload.Content, cancellationToken);
            replaced = await resumes.CompleteAsync(claim.Id, status, draft, cancellationToken);
        }
        catch
        {
            await files.DeleteAsync(claim.WorkspaceId, claim.Id, CancellationToken.None);
            await resumes.ReleaseAsync(claim.Id, CancellationToken.None);
            throw;
        }

        foreach (var resumeId in replaced)
            await files.DeleteAsync(claim.WorkspaceId, resumeId, cancellationToken);

        return ResumeUploadOutcome.Completed(new ResumeView(claim.FileName, now, status, draft));
    }

    public async Task<ResumeView?> FindCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var current = await resumes.FindCurrentAsync(workspace.WorkspaceId, cancellationToken);
        return current is null ? null : View(current);
    }

    public async Task<ResumeFile?> OpenCurrentFileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var workspace = await memberships.RequireWorkspaceAsync(userId, cancellationToken);
        var current = await resumes.FindCurrentAsync(workspace.WorkspaceId, cancellationToken);
        if (current is null) return null;
        var content = await files.OpenAsync(workspace.WorkspaceId, current.Id, cancellationToken);
        return content is null ? null : new ResumeFile(current.FileName, content);
    }

    private async Task<(ResumeStatus, ResumeDraft?)> ExtractAsync(byte[] pdf, CancellationToken cancellationToken)
    {
        var text = pdfs.ReadText(pdf);
        if (string.IsNullOrWhiteSpace(text)) return (ResumeStatus.Unreadable, null);
        var facts = await extractor.ExtractAsync(text, cancellationToken);
        return facts is null
            ? (ResumeStatus.ExtractionFailed, null)
            : (ResumeStatus.Extracted, ResumeDraftPolicy.From(facts, DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime)));
    }

    private static ResumeView View(StoredResume resume) =>
        new(resume.FileName, resume.ClaimedAt, resume.Status, resume.Draft);
}
