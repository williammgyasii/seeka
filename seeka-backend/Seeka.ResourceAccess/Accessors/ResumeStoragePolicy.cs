namespace Seeka.ResourceAccess.Accessors;

public static class ResumeStoragePolicy
{
    public static ResumeStorage Choose(string environmentName, string? accessKeyId)
    {
        if (!string.IsNullOrWhiteSpace(accessKeyId)) return ResumeStorage.R2;
        return string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase)
            ? ResumeStorage.Local
            : ResumeStorage.Unconfigured;
    }

    public static string Key(Guid workspaceId, Guid resumeId) => $"resumes/{workspaceId}/{resumeId}.pdf";
}
