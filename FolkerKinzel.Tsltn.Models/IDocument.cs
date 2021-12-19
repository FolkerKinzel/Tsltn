using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IDocument : INotifyPropertyChanged
    {
        bool Changed { get; }
        INode? FirstNode { get; }
        string? SourceDocumentFileName { get; set; }
        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }

        IEnumerable<KeyValuePair<long, string>> GetAllTranslations();

        void RemoveTranslation(long id);

        //bool IsValidXml(string translation, [NotNullWhen(false)] out string? exceptionMessage);

        //void ChangeSourceDocumentFileName(string? fileName);


        //ConcurrentBag<Task> Tasks { get; }

        //Task WaitAllTasks();


    }
}