using Seeka.Engines.Resumes;
using Seeka.Managers.Contracts.Resumes;

namespace Seeka.Managers.Access.Resumes;

public sealed record StoredResume(
    Guid Id,
    Guid WorkspaceId,
    string FileName,
    DateTimeOffset ClaimedAt,
    ResumeStatus Status,
    ResumeDraft? Draft);
