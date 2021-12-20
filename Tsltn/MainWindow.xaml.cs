using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using Tsltn.Resources;

namespace Tsltn
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<TranslationErrorsEventArgs>? TranslationErrors;

        private readonly IRecentFilesMenu _recentFilesMenu;
        private bool _isCommandEnabled = true;
        private readonly ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();

        public MainWindow(IFileController fileController, IRecentFilesMenu recentFilesMenu)
        {
            Controller = fileController;
            InitializeComponent();

            _recentFilesMenu = recentFilesMenu;

            _miGitHub.Header = string.Format(CultureInfo.CurrentCulture, Res.OnlineHelpMenuHeader, App.ProgramName);
        }


        public void Dispose()
        {
            Controller.Dispose();
            _recentFilesMenu.Dispose();
        }


        public bool IsCommandEnabled
        {
            get => _isCommandEnabled;
            private set
            {
                _isCommandEnabled = value;
                OnPropertyChanged();
            }
        }

        public IFileController Controller { get; }


        #region EventHandler

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Controller.HasContentChanged += FileController_HasContentChanged;
            Controller.Message += FileController_Message;
            Controller.NewFileName += FileController_NewFileName;
            //Controller.PropertyChanged += _fileController_PropertyChanged;
            Controller.RefreshData += FileController_RefreshData;
            Controller.ShowFileDialog += FileController_ShowFileDialog;
            Controller.BadFileName += FileController_BadFileName;
            _recentFilesMenu.Initialize(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;

            _ = ProcessCommandLineArgs();
        }


        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!await Controller.CloseDocumentAsync().ConfigureAwait(true))
            {
                e.Cancel = true;
            }
        }

        //[SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void Window_Closed(object sender, EventArgs e)
        {
            await Task.WhenAll(_tasks).ConfigureAwait(false);
            Dispose();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FileController_BadFileName(object? sender, BadFileNameEventArgs e)
            => _tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));

        private void FileController_ShowFileDialog(object? sender, ShowFileDialogEventArgs e) =>
            Dispatcher.Invoke(() => e.ShowDialog(this), DispatcherPriority.Send);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FileController_RefreshData(object? sender, EventArgs e) => Dispatcher.Invoke(RefreshData);



        //private void _fileController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(Controller.FileName))
        //    {
        //        OnPropertyChanged(nameof(FileName));
        //    }
        //}


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FileController_NewFileName(object? sender, NewFileNameEventArgs e)
            => _tasks.Add(_recentFilesMenu.AddRecentFileAsync(e.FileName));


        private void FileController_HasContentChanged(object? sender, HasContentChangedEventArgs e)
            => Dispatcher.BeginInvoke(() => ChangeContent(e.HasContent));

        private void ChangeContent(bool showContent)
        {
            if (showContent)
            {
                Debug.Assert(Controller.CurrentDocument != null);
                var cntr = new TsltnControl(this, Controller.CurrentDocument);
                _ccContent.Content = cntr;
                _ = cntr._tbOriginal.Focus();
            }
            else
            {
                _ccContent.Content = null;
            }
        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void FileController_Message(object? sender, MessageEventArgs e)
            => e.Result = Dispatcher.Invoke(() => MessageBox.Show(this, e.Message, App.ProgramName, e.Button, e.Image, e.DefaultResult), DispatcherPriority.Send);


        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                _ = Controller.LoadDocumentAsync(e.FileName);
            }
            else
            {
                _tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
            }
        }


        private void miQuit_Click(object sender, RoutedEventArgs e) => Close();


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

            _ = MessageBox.Show(sb.ToString(), $"{App.ProgramName} - {Res.About}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MiSave_Loaded(object sender, RoutedEventArgs e) => RefreshData();

        #endregion

        #region Commands

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            new HelpWindow().Show();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            _ = Controller.NewDocumentAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            _ = Controller.LoadDocumentAsync(null);
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && Controller.CurrentDocument?.SourceDocumentFileName != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Close_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            _ = Controller.CloseDocumentAsync();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && (Controller.CurrentDocument?.Changed ?? false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            IsCommandEnabled = false;
            await Controller.SaveDocumentAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            IsCommandEnabled = false;
            await Controller.SaveAsTsltnAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            IsCommandEnabled = false;
            Controller.SuspendSourceFileObservation();
            e.Handled = true;

            (IEnumerable<DataError> Errors, IEnumerable<KeyValuePair<long, string>> UnusedTranslations) = await Controller.TranslateAsync().ConfigureAwait(true);

            IsCommandEnabled = true;

            OnTranslationErrors(Errors);


            if (UnusedTranslations.Any())
            {
                var wnd = new SelectUnusedTranslationsWindow(System.IO.Path.GetFileName(Controller.FileName), UnusedTranslations);

                if (true == wnd.ShowDialog(this))
                {
                    foreach (UnusedTranslationUserControl cntr in wnd.Controls)
                    {
                        if (cntr.Remove)
                        {
                            Controller.CurrentDocument?.RemoveTranslation(cntr.Kvp.Key);
                        }
                    }
                }
            }

            Controller.ResumeSourceFileObservation();
        }

        private async void ChangeSourceDocument_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            string? sourceFilePath = Controller.CurrentDocument?.SourceDocumentFileName;
            var dlg = new OpenFileDialog()
            {
                FileName = Path.GetFileName(sourceFilePath) ?? "",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = ".xml",
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                DereferenceLinks = true,
                InitialDirectory = Path.GetDirectoryName(sourceFilePath) ?? "",
                Multiselect = false,
                ValidateNames = true,
                Title = Res.NewSourceFile

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };


            if (dlg.ShowDialog() == true)
            {
                await Controller.ChangeSourceDocumentAsync(dlg.FileName).ConfigureAwait(true);

                if (_ccContent.Content is TsltnControl control)
                {
                    control.RefreshSourceFileName();
                }
            }
        }

        #endregion


        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        private void OnTranslationErrors(IEnumerable<DataError> errors) => TranslationErrors?.Invoke(this, new TranslationErrorsEventArgs(errors));



        private void RefreshData()
        {
            if (_ccContent.Content is TsltnControl control)
            {
                control.UpdateSource();
            }
        }


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async Task ProcessCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            if (args.Length > 1)
            {
                IsCommandEnabled = false;
                await Controller.OpenTsltnFromCommandLineAsync(args[1]).ConfigureAwait(true);
                IsCommandEnabled = true;
            }
        }


    }
}
