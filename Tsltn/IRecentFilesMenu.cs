using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FolkerKinzel.WpfTools
{
    public interface IRecentFilesMenu
    {
        event EventHandler<RecentFileSelectedEventArgs>? RecentFileSelected;

        Task AddRecentFileAsync(string fileName);
        string? GetMostRecentFile();
        Task RemoveRecentFileAsync(string fileName);
        Task InitializeAsync(MenuItem miRecentFiles);
    }
}