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

        void AddAutoTranslation(XText node, string translatedText);
        void AddManualTranslation(XText node, string translatedText);
        string? GetTranslation(XText node);
        void RemoveManualTranslation(XText node);
    }
}