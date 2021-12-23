using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Models;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow
    {
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Controller.PropertyChanged += FileController_PropertyChanged;

            _recentFilesMenu.Initialize(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;

            _ = ProcessCommandLineArgs();
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!await CloseCurrentDocumentAsync().ConfigureAwait(true))
            {
                e.Cancel = true;
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                await Task.WhenAll(_tasks).ConfigureAwait(false);
            }
            catch { }

            Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MiQuit_Click(object sender, RoutedEventArgs e) => Close();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MiSave_Loaded(object sender, RoutedEventArgs e) => RefreshData();


        private void Info_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(64)
            .Append(App.ProgramName)
            .Append(Environment.NewLine)
            .Append("Version: ")
            .Append(((AssemblyFileVersionAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute)))?.Version)
            .AppendLine()
            .AppendLine()
            .Append(((AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute)))?.Copyright);

            _ = MessageBox.Show(this, sb.ToString(), $"{App.ProgramName} - {Res.About}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void OnlineHelp_Click(object sender, RoutedEventArgs e)
        {
            _miGitHub.IsEnabled = false;

            await Task.Run(() =>
            {
                try
                {
                    // hack because of this: https://github.com/dotnet/corefx/issues/10361
                    _ = Process.Start(new ProcessStartInfo("cmd", $"/c start {App.OnlineHelpUrl.Replace("&", "^&", StringComparison.Ordinal)}") { CreateNoWindow = true });
                }
                catch
                { }
            }).ConfigureAwait(true);

            _miGitHub.IsEnabled = true;
        }
        

        private void FileController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Controller.CurrentDocument))
            {
                _ = Dispatcher.BeginInvoke(() => ShowCurrentDocument());
            }
        }

        private void Doc_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IDocument.FileName))
            {
                string? fileName = ((IDocument)sender).FileName;
                if (fileName != null)
                {
                    _tasks.Add(_recentFilesMenu.AddRecentFileAsync(fileName));
                }
            }
        }

        private void Doc_SourceDocumentChanged(object? sender, FileSystemEventArgs e)
        {
            string message = string.Format(CultureInfo.InvariantCulture, Res.SourceDocumentChanged, Environment.NewLine);

            if(_sourceDocumentChangedTask.Status == TaskStatus.Running)
            {
                return;
            }

            string? fileName = Controller.CurrentDocument?.FileName;

            if (fileName != null)
            {
                _sourceDocumentChangedTask = Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBoxResult result = MessageBox.Show(this, message, App.ProgramName, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                        if (result == MessageBoxResult.Yes)
                        {
                            _ = OpenDocumentAsync(fileName);
                        }
                    }, DispatcherPriority.Send);

                });
            }
        }

        private void Doc_FileWatcherFailed(object? sender, ErrorEventArgs e)
        {
            string errorMessage =
                string.Format(CultureInfo.InvariantCulture, Res.FileWatcherFailed, Environment.NewLine);

            _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
        }

        private void Doc_SourceDocumentDeleted(object? sender, FileSystemEventArgs e)
        {
            string message = string.Format(CultureInfo.CurrentCulture, Res.SourceFileDeleted, Environment.NewLine, e.FullPath);
            _ = Dispatcher.BeginInvoke(() => ShowMessage(message, MessageBoxImage.Warning), DispatcherPriority.Send);
        }


        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(e.FileName, Controller.CurrentDocument?.FileName))
                {
                    ShowMessage(Res.FileAlreadyOpen, MessageBoxImage.Asterisk);
                    return;
                }

                _ = OpenDocumentAsync(e.FileName);
            }
            else
            {
                _tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
            }
        }



    }

}
