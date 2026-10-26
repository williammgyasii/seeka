using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Seeka.Managers.Access.Resumes;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Seeka.ResourceAccess.Accessors;

internal sealed class PdfTextAccess(ILogger<PdfTextAccess>? logger = null) : IPdfTextAccess
{
    private readonly ILogger logger = logger ?? NullLogger<PdfTextAccess>.Instance;

    public string ReadText(byte[] pdf)
    {
        try
        {
            using var document = PdfDocument.Open(pdf);
            return string.Join(
                Environment.NewLine,
                document.GetPages().Select(page => ContentOrderTextExtractor.GetText(page))).Trim();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "A résumé PDF could not be parsed.");
            return "";
        }
    }
}
