using Microsoft.Extensions.Logging;
using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;

namespace Seeka.ResourceAccess.Accessors;

/// <summary>Used when no OpenAI key is configured, so uploads fall back to the manual form.</summary>
internal sealed class MissingResumeExtractionAccess(ILogger<MissingResumeExtractionAccess> logger)
    : IResumeExtractionAccess
{
    public Task<ResumeFacts?> ExtractAsync(string resumeText, CancellationToken cancellationToken)
    {
        logger.LogError("OpenAI:ApiKey is not configured; résumé extraction is unavailable.");
        return Task.FromResult<ResumeFacts?>(null);
    }
}
