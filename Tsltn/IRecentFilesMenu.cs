using System;
using System.Windows.Controls;

namespace Tsltn
{
    public interface IRecentFilesMenu
    {
        event EventHandler<RecentFileSelectedEventArgs>? RecentFileSelected;

        void AddRecentFile(string fileName);
        string? GetMostRecentFile();
        void RemoveRecentFile(string fileName);
        void SetRecentFilesMenuItem(MenuItem miRecentFiles);
    }
}