using Seeka.Engines.Resumes;

namespace Seeka.Managers.Access.Resumes;

public interface IResumeExtractionAccess
{
    /// <summary>Facts stated in the résumé text, or null when the extractor failed.</summary>
    Task<ResumeFacts?> ExtractAsync(string resumeText, CancellationToken cancellationToken);
}
