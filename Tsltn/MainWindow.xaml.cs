using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
using FolkerKinzel.RecentFiles.WPF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tsltn.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Tsltn
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<TranslationErrorsEventArgs>? TranslationErrors;

        private readonly IDocument _doc;
        private readonly IFileController _fileController;
        private readonly IRecentFilesMenu _recentFilesMenu;
        private bool _isCommandEnabled = true;

        public MainWindow(IDocument doc, IFileController fileController, IRecentFilesMenu recentFilesMenu)
        {
            this._doc = doc;
            this._fileController = fileController;
            InitializeComponent();

            this._recentFilesMenu = recentFilesMenu;

            _miGitHub.Header = string.Format(CultureInfo.CurrentCulture, Res.OnlineHelpMenuHeader, App.ProgramName);
        }

        public void Dispose()
        {
            _fileController.Dispose();
            _recentFilesMenu.Dispose();
        }

        public string FileName => _fileController.FileName;

        public bool IsCommandEnabled
        {
            get => _isCommandEnabled;
            private set
            {
                _isCommandEnabled = value;
                OnPropertyChanged(nameof(IsCommandEnabled));
            }
        }


        #region EventHandler

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _fileController.HasContentChanged += _fileController_HasContentChanged;
            _fileController.Message += _fileController_Message;
            _fileController.NewFileName += _fileController_NewFileName;
            _fileController.PropertyChanged += _fileController_PropertyChanged;
            _fileController.RefreshData += _fileController_RefreshData;
            _fileController.ShowFileDialog += _fileController_ShowFileDialog;
            _fileController.BadFileName += _fileController_BadFileName;
            _recentFilesMenu.Initialize(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;

            _ = ProcessCommandLineArgs();
        }


        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!await _fileController.CloseTsltnAsync().ConfigureAwait(true))
            {
                e.Cancel = true;
            }
        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void Window_Closed(object sender, EventArgs e)
        {
            await _doc.WaitAllTasks().ConfigureAwait(false);
            Dispose();
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _fileController_BadFileName(object? sender, BadFileNameEventArgs e)
        {
            _doc.Tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
        }

        private void _fileController_ShowFileDialog(object? sender, ShowFileDialogEventArgs e)
        {
            //e.Result = e.Dialog.ShowDialog(this);

            Dispatcher.Invoke(() => e.ShowDialog(this), DispatcherPriority.Send);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _fileController_RefreshData(object? sender, EventArgs e) => Dispatcher.Invoke(RefreshData);



        private void _fileController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_fileController.FileName))
            {
                OnPropertyChanged(nameof(FileName));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _fileController_NewFileName(object? sender, NewFileNameEventArgs e)
        {
            _doc.Tasks.Add(_recentFilesMenu.AddRecentFileAsync(e.FileName));
        }


        private void _fileController_HasContentChanged(object? sender, HasContentChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() => ChangeContent(e.HasContent));
        }

        private void ChangeContent(bool showContent)
        {
            if (showContent)
            {
                var cntr = new TsltnControl(this, _doc);
                _ccContent.Content = cntr;
                cntr._tbOriginal.Focus();
            }
            else
            {
                _ccContent.Content = null;
            }
        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void _fileController_Message(object? sender, MessageEventArgs e)
        {
            e.Result = Dispatcher.Invoke(() => MessageBox.Show(this, e.Message, App.ProgramName, e.Button, e.Image, e.DefaultResult), DispatcherPriority.Send);
        }


        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                _ = _fileController.OpenTsltnAsync(e.FileName);
            }
            else
            {
                _doc.Tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
            }
        }


        private void miQuit_Click(object sender, RoutedEventArgs e) => this.Close();


        private void Info_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(64);
            sb.Append(App.ProgramName);
            sb.Append(Environment.NewLine);
            sb.Append("Version: ");
            sb.Append(((AssemblyFileVersionAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute)))?.Version);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(((AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute)))?.Copyright);

            MessageBox.Show(sb.ToString(), $"{App.ProgramName} - {Res.About}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
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
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {App.OnlineHelpUrl.Replace("&", "^&", StringComparison.Ordinal)}") { CreateNoWindow = true });
                }
                catch
                { }
            }).ConfigureAwait(true);

            _miGitHub.IsEnabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _miSave_Loaded(object sender, RoutedEventArgs e) => RefreshData();

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
            _ = _fileController.NewTsltnAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            _ = _fileController.OpenTsltnAsync(null);
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && _doc.SourceDocumentFileName != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Close_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            _ = _fileController.CloseTsltnAsync();
        }

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && _doc.Changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            IsCommandEnabled = false;
            await _fileController.SaveTsltnAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            IsCommandEnabled = false;
            await _fileController.SaveAsTsltnAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            IsCommandEnabled = false;
            _fileController.SuspendSourceFileObservation();
            e.Handled = true;

            var (Errors, UnusedTranslations) = await _fileController.TranslateAsync().ConfigureAwait(true);

            IsCommandEnabled = true;

            OnTranslationErrors(Errors);

        
            if (UnusedTranslations.Any())
            {
                var wnd = new SelectUnusedTranslationsWindow(System.IO.Path.GetFileName(_fileController.FileName), UnusedTranslations);

                if (true == wnd.ShowDialog(this))
                {
                    foreach (var cntr in wnd.Controls)
                    {
                        if (cntr.Remove)
                        {
                            _doc.RemoveTranslation(cntr.Kvp.Key);
                        }
                    }
                }
            }

            _fileController.ResumeSourceFileObservation();
        }

        private async void ChangeSourceDocument_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                FileName = _doc.SourceDocumentFileName,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = ".xml",
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true,
                Title = Res.NewSourceFile

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };


            if (dlg.ShowDialog() == true)
            {
                await _fileController.ChangeSourceDocumentAsync(dlg.FileName).ConfigureAwait(true);

                if (_ccContent.Content is TsltnControl control)
                {
                    control.RefreshSourceFileName();
                }
            }
        }

        #endregion


        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

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
                await _fileController.OpenTsltnFromCommandLineAsync(args[1]).ConfigureAwait(true);
                IsCommandEnabled = true;
            }
        }

        
    }
}
