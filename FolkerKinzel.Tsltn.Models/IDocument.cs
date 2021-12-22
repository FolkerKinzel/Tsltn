using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IDocument : INotifyPropertyChanged
    {
        string? FileName { get; }
        bool Changed { get; }
        INode? FirstNode { get; }
        string? SourceDocumentFileName { get; }

        public bool HasSourceDocument { get; }


        bool HasValidSourceDocument { get; }


        string? SourceLanguage { get; set; }
        string? TargetLanguage { get; set; }

        IEnumerable<KeyValuePair<long, string>> GetAllTranslations();

        bool HasError { get; }

        void Save(string tsltnFileName);
    }
}