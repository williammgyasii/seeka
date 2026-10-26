namespace Seeka.Managers.Access.Resumes;

public interface IPdfTextAccess
{
    /// <summary>The document's text, or empty when it has none or cannot be parsed.</summary>
    string ReadText(byte[] pdf);
}
