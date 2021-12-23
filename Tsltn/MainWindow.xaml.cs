using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Controllers;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, IDisposable                //, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorEventArgs>? TranslationError;

        private readonly IRecentFilesMenu _recentFilesMenu;
        private readonly ConcurrentBag<Task> _tasks = new();

        private Task _sourceDocumentChangedTask = Task.CompletedTask;

        public MainWindow(IFileController fileController, IRecentFilesMenu recentFilesMenu)
        {
            Controller = fileController;
            InitializeComponent();

            _recentFilesMenu = recentFilesMenu;

            _miGitHub.Header = string.Format(CultureInfo.InvariantCulture, Res.OnlineHelpMenuHeader, App.ProgramName);
        }

        public void Dispose()
        {
            Controller.Dispose();
            _recentFilesMenu.Dispose();
        }

        public IFileController Controller { get; }

    }
}
