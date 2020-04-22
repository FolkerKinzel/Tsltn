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

        IEnumerable<KeyValuePair<long, string>> GetAllTranslations();
        
        void RemoveTranslation(long id);

    }
}