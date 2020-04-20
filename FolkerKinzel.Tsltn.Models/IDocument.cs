using System.Collections.Generic;
using System.Xml;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IDocument
    {
        bool Changed { get; }
        INode? FirstNode { get; }
        bool SourceDocumentExists { get; }
        string? SourceDocumentFileName { get; set; }
        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }
        string? TsltnFileName { get; }

        void Close();
        IEnumerable<KeyValuePair<long, string>> GetAllTranslations();
        void NewTsltn(string sourceDocumentFileName);
        void Open(string? tsltnFileName);
        void RemoveUnusedTranslations(IEnumerable<KeyValuePair<long, string>> unused);
        void SaveTsltnAs(string tsltnFileName);
        void Translate(string outFileName, out List<(XmlException Exception, INode Node)> errors, out List<KeyValuePair<long, string>> unused);
    }
}