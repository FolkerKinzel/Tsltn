using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        List<(bool IsManualTranslation, int Hash, string Text)> GetAllTranslations();
        void NewTsltn(string sourceDocumentFileName);
        void Open(string? tsltnFileName);
        void RemoveUnusedTranslations(IEnumerable<(bool IsManualTranslation, int Hash, string Text)> unused);
        //void SaveTsltn();
        void SaveTsltnAs(string tsltnFileName);
        void Translate(string outFileName, out List<(XmlException Exception, INode Node)> errors, out List<(bool IsManualTranslation, int Hash, string Text)> unused);
    }
}