using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Controllers
{
    public interface IFileController
    {
        string FileName { get; }
        ConcurrentBag<Task> Tasks { get; }

        event EventHandler<HasContentChangedEventArgs>? HasContentChanged;
        event EventHandler<MessageEventArgs>? Message;
        event EventHandler<NewFileNameEventArgs>? NewFileName;
        public event EventHandler<BadFileNameEventArgs>? BadFileName;
        event PropertyChangedEventHandler? PropertyChanged;
        event EventHandler? RefreshData;
        event EventHandler<ShowFileDialogEventArgs>? ShowFileDialog;

        Task<bool> CloseTsltnAsync();
        Task NewTsltnAsync();
        Task OpenTsltnFromCommandLineAsync(string commandLineArg);
        Task OpenTsltnAsync(string? fileName);
        Task<bool> SaveTsltnAsync();
        Task<bool> SaveAsTsltnAsync();
        Task<(IEnumerable<DataError> Errors, IEnumerable<KeyValuePair<long, string>> UnusedTranslations)> TranslateAsync();
    }
}