namespace Seeka.Managers.Contracts.Resumes;

public sealed record ResumeUpload(Guid? IdempotencyKey, string? FileName, byte[] Content);
