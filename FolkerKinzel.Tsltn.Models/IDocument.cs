using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IDocument
    {
        bool Changed { get; }
        INode? FirstNode { get; }
        string? SourceDocumentFileName { get; }
        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }
        string? TsltnFileName { get; }

        void Close();
        IEnumerable<KeyValuePair<long, string>> GetAllTranslations();
        void NewTsltn(string sourceDocumentFileName);
        bool Open(string? tsltnFileName);
        void RemoveTranslation(long id);
        void SaveTsltnAs(string tsltnFileName);
        void Translate(
            string outFileName,
            string invalidXml,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations);
        bool ReloadSourceDocument(string fileName);
    }
}