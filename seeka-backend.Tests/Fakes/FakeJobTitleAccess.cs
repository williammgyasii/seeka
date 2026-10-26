using Seeka.Engines.Profiles;
using Seeka.Managers.Access.Candidates;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeJobTitleAccess : IJobTitleAccess
{
    public Task<JobTitleCatalog> LoadAsync(CancellationToken cancellationToken) =>
        Task.FromResult(TestJobTitles.Catalog);
}
