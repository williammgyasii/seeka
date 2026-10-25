using Microsoft.EntityFrameworkCore;
using Seeka.Engines.Outreach;
using Seeka.Managers.Access.Outreach;
using Seeka.Managers.Access.Pipeline;
using Seeka.Managers.Contracts.Outreach;
using Seeka.ResourceAccess.Mapping;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class OutreachAccess(SeekaDbContext db) : IOutreachAccess
{
    public async Task<ContactView?> AddContactAsync(
        Guid workspaceId,
        Guid savedJobId,
        NewContact contact,
        ActivityEntry activity,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
        if (savedJob?.JobPosting.CompanyId is null) return null;
        var row = new Contact
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            CompanyId = savedJob.JobPosting.CompanyId.Value,
            Name = contact.Name,
            Title = contact.Title,
            ProfessionalProfileUrl = contact.ProfessionalProfileUrl,
            ProfessionalEmail = contact.ProfessionalEmail,
            EmailSource = contact.EmailSource,
            VerificationStatus = contact.VerificationStatus,
            RelevanceReason = contact.RelevanceReason,
            CreatedAt = contact.CreatedAt
        };
        db.Contacts.Add(row);
        db.PipelineActivities.Add(activity.ToRow(workspaceId, savedJobId));
        await db.SaveChangesAsync(cancellationToken);
        return row.ToView();
    }

    public async Task<DraftTarget?> FindDraftTargetAsync(
        Guid workspaceId,
        Guid savedJobId,
        Guid contactId,
        CancellationToken cancellationToken)
    {
        var savedJob = await db.SavedJobs.AsNoTracking()
            .Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
        var contact = await db.Contacts.AsNoTracking().SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == contactId,
            cancellationToken);
        return savedJob is null || contact is null ||
               contact.CompanyId != savedJob.JobPosting.CompanyId
            ? null
            : new DraftTarget(savedJob.JobPosting.Title, savedJob.JobPosting.CompanyName, contact.Name);
    }

    public async Task<IReadOnlyList<OutreachDraftView>> SaveDraftsAsync(
        Guid workspaceId,
        Guid savedJobId,
        Guid contactId,
        IReadOnlyList<GeneratedDraft> drafts,
        ActivityEntry activity,
        CancellationToken cancellationToken)
    {
        var rows = drafts.Select(item => new OutreachDraft
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            SavedJobId = savedJobId,
            ContactId = contactId,
            Channel = item.Channel,
            Subject = item.Subject,
            Body = item.Body,
            CreatedAt = activity.OccurredAt,
            UpdatedAt = activity.OccurredAt
        }).ToArray();
        db.OutreachDrafts.AddRange(rows);
        db.PipelineActivities.Add(activity.ToRow(workspaceId, savedJobId));
        await db.SaveChangesAsync(cancellationToken);
        return rows.Select(item => item.ToView()).ToArray();
    }

    public async Task<bool> UpdateDraftAsync(
        Guid workspaceId,
        Guid draftId,
        DraftChanges changes,
        CancellationToken cancellationToken)
    {
        var draft = await db.OutreachDrafts.SingleOrDefaultAsync(
            item => item.WorkspaceId == workspaceId && item.Id == draftId,
            cancellationToken);
        if (draft is null) return false;
        draft.Subject = changes.Subject;
        draft.Body = changes.Body;
        draft.IsApproved = changes.IsApproved;
        draft.UpdatedAt = changes.UpdatedAt;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
