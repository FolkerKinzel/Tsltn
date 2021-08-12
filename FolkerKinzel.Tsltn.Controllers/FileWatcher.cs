using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Controllers
{
    public sealed class FileWatcher : IDisposable, IFileWatcher
    {
        private bool _raiseEvents = true;
        private readonly object _lockObject = new object();

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher()
        {
            NotifyFilter =
            NotifyFilters.FileName |
            //NotifyFilters.DirectoryName |
            NotifyFilters.LastWrite |
            NotifyFilters.LastAccess,
        };

        private string? _watchedFile;

        public event EventHandler<EventArgs>? Reload;


        private FileWatcher()
        {
            _watcher.Changed += Watcher_Changed;
            _watcher.Renamed += Watcher_Renamed;
            //_watcher.Created += Watcher_Changed;
            //_watcher.Deleted += Watcher_Changed;

        }

        public static FileWatcher Instance { get; } = new FileWatcher();

        public bool RaiseEvents
        {
            get
            {
                lock (_lockObject)
                {
                    return _raiseEvents;
                }
            }
            set
            {
                lock (_lockObject)
                {
                    _raiseEvents = value;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
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
                            _raiseEvents = true;
                            _watcher.EnableRaisingEvents = true;
                        }
                        return;
                    }
                    catch { }
                }

                lock (_lockObject)
                {
                    _watchedFile = value;
                    _watcher.EnableRaisingEvents = false;
                    _raiseEvents = false;
                }
            }
        }

        public void Dispose()
        {
            ((IDisposable)_watcher).Dispose();
        }


        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
#endif
            WatchedFile = e.FullPath;
            _ = OnReloadAsync();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"{e.ChangeType}: {e.FullPath}");
#endif

            _ = OnReloadAsync();
        }


        private Task OnReloadAsync()
        {
            if (RaiseEvents)
            {
                return Task.Run(() =>
                {
                    RaiseEvents = false;
                    Reload?.Invoke(this, EventArgs.Empty);
                    RaiseEvents = true;
                });
            }

            return Task.CompletedTask;
        }
    }




}
