namespace Seeka.Engines.Resumes;

public static class ResumeUploadPolicy
{
    public const long MaxBytes = 5 * 1024 * 1024;

    /// <summary>A claim older than this belongs to a request that crashed and may be retaken.</summary>
    public static readonly TimeSpan ClaimLifetime = TimeSpan.FromMinutes(5);

    private static ReadOnlySpan<byte> PdfSignature => "%PDF-"u8;

    public static UploadRejection? Check(Guid? idempotencyKey, long length, ReadOnlySpan<byte> header)
    {
        if (idempotencyKey is null || idempotencyKey == Guid.Empty)
            return new("idempotencyKey", "Send an Idempotency-Key header with a UUID.");
        if (length == 0)
            return new("file", "Choose a résumé to upload.");
        if (length > MaxBytes)
            return new("file", "Upload a PDF of 5 MB or less.");
        if (!header.StartsWith(PdfSignature))
            return new("file", "Upload your résumé as a PDF.");
        return null;
    }

    public static string DisplayName(string? fileName)
    {
        var name = (fileName ?? "").Replace('\\', '/').Split('/')[^1].Trim();
        return name.Length == 0 ? "resume.pdf" : name[..Math.Min(name.Length, 120)];
    }

    public static bool IsAbandoned(DateTimeOffset claimedAt, DateTimeOffset now) =>
        now - claimedAt > ClaimLifetime;
}
