using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Controllers
{
    public interface IFileController : IDisposable
    {

        //event EventHandler<HasContentChangedEventArgs>? HasContentChanged;
        event EventHandler<MessageEventArgs>? Message;
        //event EventHandler<NewFileNameEventArgs>? NewFileName;
        //event EventHandler<BadFileNameEventArgs>? BadFileName;
        event PropertyChangedEventHandler? PropertyChanged;
        event EventHandler? RefreshData;
        event EventHandler<ShowFileDialogEventArgs>? ShowFileDialog;
        event EventHandler<DataErrorEventArgs>? TranslationError;
        event EventHandler<UnusedTranslationEventArgs>? UnusedTranslations;
        event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;


        IDocument? CurrentDocument { get; }

        void CloseCurrentDocument();
        void NewDocument(string xmlFileName);
        Task OpenTsltnFromCommandLineAsync(string commandLineArg);
        void OpenTsltnDocument(string tsltnFileName);
        //Task<bool> SaveDocumentAsync();
        //Task<bool> SaveAsTsltnAsync();
        Task TranslateAsync();

        //void SuspendSourceFileObservation();
        //void ResumeSourceFileObservation();

        Task ChangeSourceDocumentAsync(string? newSourceDocument);
    }
}