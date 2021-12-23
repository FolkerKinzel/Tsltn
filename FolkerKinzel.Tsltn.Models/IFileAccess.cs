using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess : IDocument, IDisposable
    {
        event EventHandler<ErrorEventArgs>? FileWatcherFailed;
        event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
        event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;






    }
}
