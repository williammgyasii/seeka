using Seeka.Managers.Access.Resumes;

namespace Seeka.Backend.Tests.Fakes;

internal sealed class FakePdfTextAccess(string text = "Ama Mensah, HR Manager. Recruitment, Payroll.") : IPdfTextAccess
{
    public string Text { get; set; } = text;

    public string ReadText(byte[] pdf) => Text;
}
