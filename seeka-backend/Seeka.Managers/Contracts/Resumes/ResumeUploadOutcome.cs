using Seeka.Engines.Resumes;

namespace Seeka.Managers.Contracts.Resumes;

public sealed record ResumeUploadOutcome(ResumeView? Resume, UploadRejection? Rejection, bool InProgress)
{
    public static ResumeUploadOutcome Completed(ResumeView resume) => new(resume, null, false);
    public static ResumeUploadOutcome Rejected(UploadRejection rejection) => new(null, rejection, false);
    public static ResumeUploadOutcome Running() => new(null, null, true);
}
