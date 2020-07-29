using System;

namespace FolkerKinzel.Tsltn.Controllers
{
    public interface IFileWatcher
    {
        bool RaiseEvents { get; set; }
        string? WatchedFile { get; set; }

        event EventHandler<EventArgs>? Reload;

        void Dispose();
    }
}