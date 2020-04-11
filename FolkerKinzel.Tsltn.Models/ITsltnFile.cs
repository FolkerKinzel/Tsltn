using System.Xml;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public interface ITsltnFile
    {
        string? SourceDocumentFileName { get; set; }
        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }

        void AddAutoTranslation(Translation transl);
        void AddManualTranslation(ManualTranslation manual);
        Translation? GetTranslation(string originalText, string elementXPath);
        void RemoveManualTranslation(string originalText, string elementXPath);
    }
}