using Seeka.Engines.Resumes;

namespace Seeka.Managers.Contracts.Resumes;

public sealed record ResumeView(
    string FileName,
    DateTimeOffset UploadedAt,
    ResumeStatus Status,
    ResumeDraft? Draft);
