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
        event PropertyChangedEventHandler? PropertyChanged;
        event EventHandler? RefreshData;
        event EventHandler<ShowFileDialogEventArgs>? ShowFileDialog;

        Task<bool> DoCloseTsltnAsync();
        Task DoNewTsltnAsync();
        Task DoOpenAsync(string? fileName);
        Task<bool> DoSaveAsync(string? fileName);
        Task<(List<DataError> Errors, List<KeyValuePair<long, string>> UnusedTranslations)> DoTranslateAsync(string fileName);
        void RemoveUnusedTranslations(IEnumerable<long> unusedTranslations);
    }
}