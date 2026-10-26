using System.Text;
using Seeka.Engines.Resumes;

namespace Seeka.Backend.Tests;

public sealed class ResumeUploadPolicyTests
{
    private static readonly byte[] PdfHeader = Encoding.ASCII.GetBytes("%PDF-1.7");
    private static readonly Guid Key = Guid.NewGuid();

    [Fact]
    public void Accepts_a_pdf_within_the_size_limit() =>
        Assert.Null(ResumeUploadPolicy.Check(Key, ResumeUploadPolicy.MaxBytes, PdfHeader));

    [Fact]
    public void Rejects_a_missing_key()
    {
        var rejection = ResumeUploadPolicy.Check(null, 1_000, PdfHeader);

        Assert.Equal("idempotencyKey", rejection?.Field);
    }

    [Fact]
    public void Rejects_an_empty_key() =>
        Assert.Equal("idempotencyKey", ResumeUploadPolicy.Check(Guid.Empty, 1_000, PdfHeader)?.Field);

    [Fact]
    public void Rejects_a_file_over_five_megabytes() =>
        Assert.Equal(
            "Upload a PDF of 5 MB or less.",
            ResumeUploadPolicy.Check(Key, ResumeUploadPolicy.MaxBytes + 1, PdfHeader)?.Message);

    [Fact]
    public void Rejects_an_empty_file() =>
        Assert.Equal("file", ResumeUploadPolicy.Check(Key, 0, [])?.Field);

    [Theory]
    [InlineData("PK\u0003\u0004")]
    [InlineData("<html>")]
    [InlineData("%PD")]
    public void Rejects_content_that_is_not_a_pdf(string header) =>
        Assert.Equal(
            "Upload your résumé as a PDF.",
            ResumeUploadPolicy.Check(Key, 1_000, Encoding.ASCII.GetBytes(header))?.Message);

    [Theory]
    [InlineData("Ama Mensah CV.pdf", "Ama Mensah CV.pdf")]
    [InlineData("../../etc/passwd.pdf", "passwd.pdf")]
    [InlineData("C:\\Users\\ama\\cv.pdf", "cv.pdf")]
    [InlineData("  ", "resume.pdf")]
    [InlineData(null, "resume.pdf")]
    public void Keeps_only_a_plain_file_name(string? input, string expected) =>
        Assert.Equal(expected, ResumeUploadPolicy.DisplayName(input));

    [Fact]
    public void Shortens_a_long_file_name() =>
        Assert.Equal(120, ResumeUploadPolicy.DisplayName(new string('a', 300) + ".pdf").Length);

    [Fact]
    public void A_claim_is_abandoned_after_five_minutes()
    {
        var claimedAt = new DateTimeOffset(2026, 10, 25, 12, 0, 0, TimeSpan.Zero);

        Assert.False(ResumeUploadPolicy.IsAbandoned(claimedAt, claimedAt.AddMinutes(5)));
        Assert.True(ResumeUploadPolicy.IsAbandoned(claimedAt, claimedAt.AddMinutes(5).AddSeconds(1)));
    }
}
