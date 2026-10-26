using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Seeka.ResourceAccess.Accessors;

namespace Seeka.Backend.Tests;

public sealed class ResumeStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"seeka-test-{Guid.NewGuid():N}");

    [Theory]
    [InlineData("Production", "key-id", ResumeStorage.R2)]
    [InlineData("Development", "key-id", ResumeStorage.R2)]
    [InlineData("Development", null, ResumeStorage.Local)]
    [InlineData("Development", "", ResumeStorage.Local)]
    [InlineData("Production", null, ResumeStorage.Unconfigured)]
    public void Files_go_to_a_local_folder_only_in_development(
        string environmentName,
        string? accessKeyId,
        ResumeStorage expected) =>
        Assert.Equal(expected, ResumeStoragePolicy.Choose(environmentName, accessKeyId));

    [Fact]
    public void Files_are_keyed_under_their_workspace()
    {
        var workspaceId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();

        Assert.Equal($"resumes/{workspaceId}/{resumeId}.pdf", ResumeStoragePolicy.Key(workspaceId, resumeId));
    }

    [Fact]
    public async Task Local_folder_saves_opens_and_deletes()
    {
        var access = new LocalResumeFileAccess(root, NullLogger<LocalResumeFileAccess>.Instance);
        var workspaceId = Guid.NewGuid();
        var resumeId = Guid.NewGuid();

        await access.SaveAsync(workspaceId, resumeId, Encoding.ASCII.GetBytes("%PDF-1.7"), default);
        await using (var stream = await access.OpenAsync(workspaceId, resumeId, default))
            Assert.Equal("%PDF-1.7", await new StreamReader(stream!).ReadToEndAsync());

        await access.DeleteAsync(workspaceId, resumeId, default);

        Assert.Null(await access.OpenAsync(workspaceId, resumeId, default));
    }

    [Fact]
    public async Task Deleting_a_missing_file_does_not_throw() =>
        await new LocalResumeFileAccess(root, NullLogger<LocalResumeFileAccess>.Instance)
            .DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), default);

    public void Dispose()
    {
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }
}
