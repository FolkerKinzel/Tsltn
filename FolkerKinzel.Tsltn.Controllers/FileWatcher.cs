using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace FolkerKinzel.Tsltn.Controllers
{
    internal sealed class FileWatcher : IDisposable
    {
        private bool _raiseEvents = true;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Watch() => _raiseEvents = true;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendWatching() => _raiseEvents = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public string? WatchedFile
        {
            get => _watchedFile;
            set
            {
                _watchedFile = value;

                if(_watchedFile is string s)
                {
                    try
                    {
                        var directory = Path.GetDirectoryName(s);

                        if (Directory.Exists(directory))
                        {
                            _watcher.Path = directory;
                            _watcher.Filter = Path.GetFileName(_watchedFile);

                            _watcher.EnableRaisingEvents = true;
                            Watch();
                            return;
                        }
                    }
                    catch { }
                }

                _watcher.EnableRaisingEvents = false;
                SuspendWatching(); 
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

        //private void OnMessage(MessageEventArgs args) => Message?.Invoke(this, args);
        private Task OnReloadAsync()
        {
            if (_raiseEvents)
            {
                return Task.Run(() =>
                {
                    SuspendWatching();
                    Reload?.Invoke(this, EventArgs.Empty);
                    Watch();
                });
            }

            return Task.CompletedTask;
        }
    }




}
