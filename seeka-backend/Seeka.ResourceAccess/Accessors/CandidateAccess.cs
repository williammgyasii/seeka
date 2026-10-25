using Microsoft.EntityFrameworkCore;
using Seeka.Managers.Access.Candidates;
using Seeka.Managers.Contracts.Candidates;
using Seeka.ResourceAccess.Mapping;
using Seeka.ResourceAccess.Persistence.Entities;
using Seeka.ResourceAccess.Persistence;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class CandidateAccess(SeekaDbContext db) : ICandidateAccess
{
    public async Task<CandidateSetupResponse?> FindSetupAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences.AsNoTracking()
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        return profile is null || preferences is null
            ? null
            : new CandidateSetupResponse(profile.ToView(), preferences.ToView());
    }

    public async Task<CandidateSetupResponse> SaveSetupAsync(
        Guid workspaceId,
        CandidateSetupRequest setup,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var profile = await db.CandidateProfiles
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);
        var preferences = await db.JobPreferences
            .SingleOrDefaultAsync(item => item.WorkspaceId == workspaceId, cancellationToken);

        profile ??= new CandidateProfile { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
        preferences ??= new JobPreference { Id = Guid.NewGuid(), WorkspaceId = workspaceId };
        if (db.Entry(profile).State == EntityState.Detached) db.CandidateProfiles.Add(profile);
        if (db.Entry(preferences).State == EntityState.Detached) db.JobPreferences.Add(preferences);

        profile.FullName = setup.FullName;
        profile.CountryOfResidence = setup.CountryOfResidence;
        profile.TimeZone = setup.TimeZone;
        profile.Seniority = setup.Seniority;
        profile.YearsOfExperience = setup.YearsOfExperience;
        profile.ExperienceSummary = setup.ExperienceSummary;
        profile.Skills = setup.Skills;
        profile.UpdatedAt = updatedAt;

        preferences.TargetJobTitles = setup.TargetJobTitles;
        preferences.EmploymentTypes = setup.EmploymentTypes;
        preferences.AvailableWorkingHours = setup.AvailableWorkingHours;
        preferences.MinimumCompensation = setup.MinimumCompensation;
        preferences.CompensationCurrency = setup.CompensationCurrency;
        preferences.UpdatedAt = updatedAt;

        await db.SaveChangesAsync(cancellationToken);
        return new CandidateSetupResponse(profile.ToView(), preferences.ToView());
    }
}
