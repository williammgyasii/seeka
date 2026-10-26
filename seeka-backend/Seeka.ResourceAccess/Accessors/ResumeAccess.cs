using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;
using Seeka.Managers.Contracts.Resumes;
using Seeka.ResourceAccess.Persistence;
using Seeka.ResourceAccess.Persistence.Entities;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class ResumeAccess(SeekaDbContext db) : IResumeAccess
{
    public async Task<StoredResume?> FindByKeyAsync(
        Guid workspaceId,
        Guid idempotencyKey,
        CancellationToken cancellationToken)
    {
        var resume = await db.Resumes.AsNoTracking().SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.IdempotencyKey == idempotencyKey,
            cancellationToken);
        return resume is null ? null : Stored(resume);
    }

    public async Task<bool> ClaimAsync(NewResume resume, CancellationToken cancellationToken)
    {
        if (await db.Resumes.AnyAsync(
                item => item.WorkspaceId == resume.WorkspaceId && item.IdempotencyKey == resume.IdempotencyKey,
                cancellationToken))
            return false;

        var entity = new Resume
        {
            Id = resume.Id,
            WorkspaceId = resume.WorkspaceId,
            IdempotencyKey = resume.IdempotencyKey,
            FileName = resume.FileName,
            SizeBytes = resume.SizeBytes,
            Status = nameof(ResumeStatus.Pending),
            ClaimedAt = resume.ClaimedAt
        };
        db.Resumes.Add(entity);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.Entry(entity).State = EntityState.Detached;
            return false;
        }
    }

    public async Task ReleaseAsync(Guid resumeId, CancellationToken cancellationToken)
    {
        var claim = await db.Resumes.SingleOrDefaultAsync(
            item => item.Id == resumeId && item.Status == nameof(ResumeStatus.Pending),
            cancellationToken);
        if (claim is null) return;
        db.Resumes.Remove(claim);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> CompleteAsync(
        Guid resumeId,
        ResumeStatus status,
        ResumeDraft? draft,
        CancellationToken cancellationToken)
    {
        var resume = await db.Resumes.SingleAsync(item => item.Id == resumeId, cancellationToken);
        var replaced = await db.Resumes
            .Where(item => item.WorkspaceId == resume.WorkspaceId && item.IsCurrent && item.Id != resumeId)
            .ToListAsync(cancellationToken);

        resume.Status = status.ToString();
        resume.DraftJson = draft is null ? null : JsonSerializer.Serialize(draft);
        resume.IsCurrent = true;
        replaced.ForEach(item => item.IsCurrent = false);
        await db.SaveChangesAsync(cancellationToken);
        return replaced.Select(item => item.Id).ToList();
    }

    public async Task<StoredResume?> FindCurrentAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var resume = await db.Resumes.AsNoTracking()
            .Where(item => item.WorkspaceId == workspaceId && item.IsCurrent)
            .OrderByDescending(item => item.ClaimedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return resume is null ? null : Stored(resume);
    }

    private static StoredResume Stored(Resume resume) =>
        new(
            resume.Id,
            resume.WorkspaceId,
            resume.FileName,
            resume.ClaimedAt,
            Enum.Parse<ResumeStatus>(resume.Status),
            resume.DraftJson is null ? null : JsonSerializer.Deserialize<ResumeDraft>(resume.DraftJson));
}
