using Seeka.Managers.Access.Jobs;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeJobAccess : IJobAccess
{
    public ImportTarget Target { get; init; } = new(null, null);
    public string? LookedUpUrl { get; private set; }
    public JobImport? Saved { get; private set; }

    public Task<ImportTarget> FindImportTargetAsync(
        string canonicalUrl,
        string companyName,
        string? companyDomain,
        CancellationToken cancellationToken)
    {
        LookedUpUrl = canonicalUrl;
        return Task.FromResult(Target);
    }

    public Task<ImportedJob> SaveImportAsync(JobImport import, CancellationToken cancellationToken)
    {
        Saved = import;
        return Task.FromResult(new ImportedJob(Guid.NewGuid(), Guid.NewGuid()));
    }
}
