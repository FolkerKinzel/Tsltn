using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal sealed class FileWatcher : IDisposable //, IFileWatcher
    {
        //private bool _raiseEvents = true;
        private readonly object _lockObject = new object();

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher()
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = false
        };

        private string? _watchedFile;

        public event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;
        public event EventHandler<RenamedEventArgs>? SourceDocumentMoved;
        public event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
        public event EventHandler<ErrorEventArgs>? FileWatcherError;


        public FileWatcher()
        {
            _watcher.Changed += Watcher_Changed;
            _watcher.Renamed += Watcher_Renamed;
            //_watcher.Created += Watcher_Changed;
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Error += Watcher_Error;
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
#endif
            SourceDocumentDeleted?.Invoke(this, e);
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            FileWatcherError?.Invoke(this, e);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
#endif
            WatchedFile = e.FullPath;
            SourceDocumentMoved?.Invoke(this, e);
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
#endif
            SourceDocumentChanged?.Invoke(this, e);
        }

        //public bool RaiseEvents
        //{
        //    get
        //    {
        //        lock (_lockObject)
        //        {
        //            return _raiseEvents;
        //        }
        //    }
        //    set
        //    {
        //        lock (_lockObject)
        //        {
        //            _raiseEvents = value;
        //        }
        //    }
        //}

        public string? WatchedFile
        {
            get => _watchedFile;
            set
            {
                if (File.Exists(value))
                {
                    try
                    {
                        lock (_lockObject)
                        {
                            _watchedFile = value;
                            _watcher.Path = Path.GetDirectoryName(value);
                            _watcher.Filter = Path.GetFileName(value);
                            //_raiseEvents = true;
                            _watcher.EnableRaisingEvents = true;
                        }
                        return;
                    }
                    catch { }
                }

                lock (_lockObject)
                {
                    _watchedFile = null;
                    _watcher.EnableRaisingEvents = false;
                    //_raiseEvents = false;
                }
            }
        }

        public void Dispose() => ((IDisposable)_watcher).Dispose();





        //private Task OnSourceDocumentChangedAsync()
        //{
        //    if (RaiseEvents)
        //    {
        //        return Task.Run(() => {
        //            RaiseEvents = false;
        //            SourceDocumentChanged?.Invoke(this, EventArgs.Empty);
        //            RaiseEvents = true;
        //        });
        //    }

        //    return Task.CompletedTask;
        //}
    }




}
