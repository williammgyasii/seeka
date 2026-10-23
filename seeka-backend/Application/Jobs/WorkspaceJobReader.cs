using Microsoft.EntityFrameworkCore;
using Seeka.Infrastructure;
using Seeka.Infrastructure.Entities;

namespace Seeka.Application.Jobs;

public sealed class WorkspaceJobReader(SeekaDbContext db)
{
    public Task<SavedJob?> FindAsync(
        Guid workspaceId,
        Guid savedJobId,
        CancellationToken cancellationToken = default) =>
        db.SavedJobs
            .AsNoTracking()
            .Include(item => item.JobPosting)
            .SingleOrDefaultAsync(
                item => item.WorkspaceId == workspaceId && item.Id == savedJobId,
                cancellationToken);
}
