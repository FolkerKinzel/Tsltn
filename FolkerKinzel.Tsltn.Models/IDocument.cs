using System.ComponentModel;

namespace FolkerKinzel.Tsltn.Models;

public interface IDocument : INotifyPropertyChanged
{
    event EventHandler<ErrorEventArgs>? FileWatcherFailed;
    event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
    event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;

    string? FileName { get; }
    bool Changed { get; }
    INode? FirstNode { get; }
    string? SourceDocumentFileName { get; }
    bool HasSourceDocument { get; }
    bool HasValidSourceDocument { get; }
    bool HasError { get; }
    string? SourceLanguage { get; set; }
    string? TargetLanguage { get; set; }
    void ChangeSourceDocument(string xmlFileName);
    IEnumerable<KeyValuePair<long, string>> GetAllTranslations();
    void Save(string tsltnFileName);
    (IList<DataError> Errors, IList<KeyValuePair<long, string>> UnusedTranslations) Translate(string outFileName);

    void RemoveTranslation(long id);
}
