using System;
using System.IO;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileWatcher
    {
        //bool RaiseEvents { get; set; }
        string? WatchedFile { get; set; }

        public event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;
        public event EventHandler<RenamedEventArgs>? SourceDocumentMoved;
        public event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
        public event EventHandler<ErrorEventArgs>? FileWatcherError;

        void Dispose();
    }
}