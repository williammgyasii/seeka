using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Seeka.Infrastructure.Entities;

namespace Seeka.Infrastructure;

public sealed class SeekaDbContext(
    DbContextOptions<SeekaDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options),
      IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMembership> WorkspaceMemberships => Set<WorkspaceMembership>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<JobPreference> JobPreferences => Set<JobPreference>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<JobSourceRecord> JobSourceRecords => Set<JobSourceRecord>();
    public DbSet<EligibilityAssessment> EligibilityAssessments => Set<EligibilityAssessment>();
    public DbSet<MatchAssessment> MatchAssessments => Set<MatchAssessment>();
    public DbSet<SavedJob> SavedJobs => Set<SavedJob>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<OutreachDraft> OutreachDrafts => Set<OutreachDraft>();
    public DbSet<PipelineActivity> PipelineActivities => Set<PipelineActivity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Workspace>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(160);
            entity.HasIndex(item => item.PersonalOwnerUserId).IsUnique();
        });

        builder.Entity<WorkspaceMembership>(entity =>
        {
            entity.Property(item => item.Role).HasConversion<string>();
            entity.HasIndex(item => new { item.WorkspaceId, item.UserId }).IsUnique();
            entity.HasOne(item => item.Workspace)
                .WithMany(item => item.Memberships)
                .HasForeignKey(item => item.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CandidateProfile>(entity =>
        {
            entity.HasIndex(item => item.WorkspaceId).IsUnique();
            entity.Property(item => item.FullName).HasMaxLength(160);
            entity.Property(item => item.CountryOfResidence).HasMaxLength(100);
            entity.Property(item => item.TimeZone).HasMaxLength(100);
        });

        builder.Entity<JobPreference>(entity =>
        {
            entity.HasIndex(item => item.WorkspaceId).IsUnique();
            entity.Property(item => item.CompensationCurrency).HasMaxLength(3);
            entity.Property(item => item.MinimumCompensation).HasPrecision(18, 2);
        });

        builder.Entity<Company>(entity =>
        {
            entity.Property(item => item.Name).HasMaxLength(200);
            entity.Property(item => item.Domain).HasMaxLength(255);
            entity.HasIndex(item => item.Domain).IsUnique();
        });

        builder.Entity<JobPosting>(entity =>
        {
            entity.Property(item => item.AvailabilityStatus).HasConversion<string>();
            entity.Property(item => item.SalaryMinimum).HasPrecision(18, 2);
            entity.Property(item => item.SalaryMaximum).HasPrecision(18, 2);
            entity.HasIndex(item => item.CanonicalUrl).IsUnique();
            entity.HasOne(item => item.Company)
                .WithMany()
                .HasForeignKey(item => item.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<JobSourceRecord>(entity =>
        {
            entity.HasIndex(item => new { item.Provider, item.ExternalId })
                .IsUnique()
                .HasFilter("\"ExternalId\" IS NOT NULL");
            entity.HasIndex(item => new { item.Provider, item.CanonicalUrl }).IsUnique();
        });

        builder.Entity<EligibilityAssessment>(entity =>
        {
            entity.Property(item => item.Status).HasConversion<string>();
            entity.Property(item => item.Origin).HasConversion<string>();
            entity.HasIndex(item => new { item.WorkspaceId, item.JobPostingId, item.CheckedAt });
        });

        builder.Entity<MatchAssessment>(entity =>
            entity.HasIndex(item => new
            {
                item.WorkspaceId,
                item.JobPostingId,
                item.AssessedAt
            }));

        builder.Entity<SavedJob>(entity =>
        {
            entity.Property(item => item.Stage).HasConversion<string>();
            entity.Property(item => item.ApplicationStatus).HasConversion<string>();
            entity.HasIndex(item => new { item.WorkspaceId, item.JobPostingId }).IsUnique();
        });

        builder.Entity<Contact>(entity =>
        {
            entity.Property(item => item.VerificationStatus).HasConversion<string>();
            entity.HasIndex(item => new { item.WorkspaceId, item.CompanyId });
        });

        builder.Entity<OutreachDraft>(entity =>
            entity.HasIndex(item => new { item.WorkspaceId, item.SavedJobId }));

        builder.Entity<PipelineActivity>(entity =>
        {
            entity.HasIndex(item => new { item.WorkspaceId, item.SavedJobId, item.OccurredAt });
            entity.HasIndex(item => new { item.WorkspaceId, item.IdempotencyKey })
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        });
    }
}
