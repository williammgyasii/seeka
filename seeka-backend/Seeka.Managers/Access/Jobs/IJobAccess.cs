namespace Seeka.Managers.Access.Jobs;

public interface IJobAccess
{
    Task<ImportTarget> FindImportTargetAsync(
        string canonicalUrl,
        string companyName,
        string? companyDomain,
        CancellationToken cancellationToken);

    Task<ImportedJob> SaveImportAsync(JobImport import, CancellationToken cancellationToken);
}
