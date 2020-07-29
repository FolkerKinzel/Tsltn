using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Controllers
{
    internal sealed class FileWatcher : IDisposable
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


        public FileWatcher()
        {
            _watcher.Changed += Watcher_Changed;
            _watcher.Renamed += Watcher_Renamed;
            //_watcher.Created += Watcher_Changed;
            //_watcher.Deleted += Watcher_Changed;

        }

        public bool RaiseEvents
        {
            get
            {
                lock(_lockObject)
                {
                    return _raiseEvents;
                }
            }
            set
            {
                lock(_lockObject)
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
                lock (_lockObject)
                {
                    _watcher.EnableRaisingEvents = false;
                    _raiseEvents = false;
                    _watchedFile = value;
                }

                if(value is string s)
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(s);

                        if (Directory.Exists(directory))
                        {
                            lock (_lockObject)
                            {
                                _watcher.Path = directory;
                                _watcher.Filter = Path.GetFileName(value);
                                _watcher.EnableRaisingEvents = true;
                                _raiseEvents = true;
                            }
                            return;
                        }
                    }
                    catch { }
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
