using Seeka.Engines.Profiles;

namespace Seeka.Managers.Access.Candidates;

/// <summary>The ESCO occupation list, loaded once.</summary>
public interface IJobTitleAccess
{
    Task<JobTitleCatalog> LoadAsync(CancellationToken cancellationToken);
}
