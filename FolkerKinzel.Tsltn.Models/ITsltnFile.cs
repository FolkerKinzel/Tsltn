using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public interface ITsltnFile
    {
        string? SourceDocumentFileName { get; set; }
        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }

        void AddAutoTranslation(XElement node, string translatedText);
        void AddManualTranslation(XElement node, string translatedText);
        string? GetTranslation(XElement node);
        void RemoveManualTranslation(XElement node);
    }
}