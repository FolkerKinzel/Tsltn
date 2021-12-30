using System.Diagnostics;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal sealed class FileWatcher : IDisposable
{
    private readonly FileSystemWatcher? _watcher;

    public event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;
    public event EventHandler<RenamedEventArgs>? SourceDocumentMoved;
    //public event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
    public event EventHandler<ErrorEventArgs>? FileWatcherError;


    public FileWatcher(string? watchedFile)
    {
        if (watchedFile is null)
        {
            return;
        }

        _watcher = new FileSystemWatcher()
        {
            NotifyFilter = NotifyFilters.FileName
                         | NotifyFilters.DirectoryName
                     //  | NotifyFilters.LastWrite
                         | NotifyFilters.LastAccess
                        
                         ,
            Path = Path.GetDirectoryName(watchedFile),
            IncludeSubdirectories = false,
            Filter = Path.GetFileName(watchedFile),
            EnableRaisingEvents = true,
        };
        _watcher.Changed += Watcher_Changed;
        _watcher.Renamed += Watcher_Renamed;
        //_watcher.Deleted += Watcher_Deleted;
        //_watcher.Created += _watcher_Created;
        _watcher.Error += Watcher_Error;
    }

    //private void _watcher_Created(object sender, FileSystemEventArgs e)
    //{
        
    //}

    public void Dispose() => ((IDisposable?)_watcher)?.Dispose();

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        Debug.WriteLine($"{e.ChangeType}: {e.FullPath}");
        SourceDocumentChanged?.Invoke(this, e);
    }

    private void Watcher_Renamed(object sender, RenamedEventArgs e)
    {
        Debug.WriteLine($"{e.ChangeType}: {e.FullPath}");
        SourceDocumentMoved?.Invoke(this, e);
    }

    //private void Watcher_Deleted(object sender, FileSystemEventArgs e)
    //{
    //    Debug.WriteLine($"{e.ChangeType}: {e.FullPath}");
    //    //SourceDocumentDeleted?.Invoke(this, e);
    //}

    private void Watcher_Error(object sender, ErrorEventArgs e)
        => FileWatcherError?.Invoke(this, e);

}
