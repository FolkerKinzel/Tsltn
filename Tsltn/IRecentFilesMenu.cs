using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Tsltn
{
    public interface IRecentFilesMenu
    {
        event EventHandler<RecentFileSelectedEventArgs>? RecentFileSelected;

        Task AddRecentFileAsync(string fileName);
        string? GetMostRecentFile();
        Task RemoveRecentFileAsync(string fileName);
        void SetRecentFilesMenuItem(MenuItem miRecentFiles);
    }
}