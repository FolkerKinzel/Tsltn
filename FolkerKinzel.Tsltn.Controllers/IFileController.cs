using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
        event EventHandler<DataErrorEventArgs> TranslationError;
        event EventHandler<UnusedTranslationEventArgs> UnusedTranslations;

        IDocument? CurrentDocument { get; }

        Task<bool> CloseDocumentAsync();
        Task NewDocumentAsync();
        Task OpenTsltnFromCommandLineAsync(string commandLineArg);
        Task LoadDocumentAsync(string? fileName);
        Task<bool> SaveDocumentAsync();
        Task<bool> SaveAsTsltnAsync();
        Task TranslateAsync();

        //void SuspendSourceFileObservation();
        //void ResumeSourceFileObservation();

        Task ChangeSourceDocumentAsync(string? newSourceDocument);
    }
}