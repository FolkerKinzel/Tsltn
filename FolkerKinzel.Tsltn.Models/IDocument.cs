using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        bool IsValidXml(string s);


        ConcurrentBag<Task> Tasks { get; }

        Task WaitAllTasks();


    }
}