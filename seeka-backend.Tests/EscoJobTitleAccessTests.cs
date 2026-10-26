using Seeka.ResourceAccess.Accessors;

namespace Seeka.Backend.Tests;

public sealed class EscoJobTitleAccessTests
{
    [Fact]
    public async Task Ships_the_whole_esco_occupation_list()
    {
        var catalog = await new EscoJobTitleAccess().LoadAsync(default);

        Assert.True(catalog.Count > 2_500, $"Only {catalog.Count} occupations shipped.");
        Assert.Equal("human resources manager", catalog.Preferred("Human Resources Manager"));
    }
}
