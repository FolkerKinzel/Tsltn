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

        //event EventHandler<MessageEventArgs>? Message;
        event PropertyChangedEventHandler? PropertyChanged;
        //event EventHandler<DataErrorEventArgs>? TranslationError;
        //event EventHandler<UnusedTranslationEventArgs>? UnusedTranslations;
        event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;


        IDocument? CurrentDocument { get; }
        void CloseCurrentDocument();
        void NewDocument(string xmlFileName);
        //Task OpenTsltnFromCommandLineAsync(string commandLineArg);
        void OpenTsltnDocument(string tsltnFileName);
        //Task TranslateAsync();
    }
}