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

        Task<bool> DoCloseTsltnAsync();
        Task DoNewTsltnAsync();
        Task DoOpenAsync(string? fileName);
        Task<bool> DoSaveAsync(string? fileName);
        Task<(IEnumerable<DataError> Errors, IEnumerable<KeyValuePair<long, string>> UnusedTranslations)> TranslateAsync();
        void RemoveUnusedTranslations(IEnumerable<long> unusedTranslations);
    }
}