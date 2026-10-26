using System.Text;
using Seeka.ResourceAccess.Accessors;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace Seeka.Backend.Tests;

public sealed class PdfTextAccessTests
{
    private readonly PdfTextAccess access = new();

    [Fact]
    public void Reads_the_text_of_every_page()
    {
        var pdf = Pdf("Ama Mensah", "HR Manager since 2018");

        var text = access.ReadText(pdf);

        Assert.Contains("Ama Mensah", text);
        Assert.Contains("HR Manager since 2018", text);
    }

    [Fact]
    public void Returns_empty_text_for_a_pdf_without_text() =>
        Assert.Equal("", access.ReadText(Pdf()).Trim());

    [Fact]
    public void Returns_empty_text_for_a_broken_pdf() =>
        Assert.Equal("", access.ReadText(Encoding.ASCII.GetBytes("%PDF-1.7 not really a pdf")));

    private static byte[] Pdf(params string[] pages)
    {
        var builder = new PdfDocumentBuilder();
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);
        if (pages.Length == 0) builder.AddPage(PageSize.A4);
        foreach (var line in pages)
            builder.AddPage(PageSize.A4).AddText(line, 12, new PdfPoint(40, 760), font);
        return builder.Build();
    }
}
