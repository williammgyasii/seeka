using Seeka.Engines.Resumes;
using Seeka.Managers.Access.Resumes;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakeResumeExtractionAccess : IResumeExtractionAccess
{
    public ResumeFacts? Facts { get; set; } =
        new(" Ama Mensah ", "Senior", 8, "Eight years in HR.", ["Recruitment", "recruitment"], ["HR Manager"]);

    public Exception? Throw { get; set; }

    /// <summary>When set, extraction waits for it, holding the request open.</summary>
    public Task? Hold { get; set; }

    public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int Calls { get; private set; }

    public async Task<ResumeFacts?> ExtractAsync(string resumeText, CancellationToken cancellationToken)
    {
        Calls++;
        Started.TrySetResult();
        if (Hold is not null) await Hold;
        if (Throw is not null) throw Throw;
        return Facts;
    }
}
