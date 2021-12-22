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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Controllers.Enums;
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

        public event EventHandler<DataErrorEventArgs>? TranslationError;

        private readonly IRecentFilesMenu _recentFilesMenu;
        private bool _isCommandEnabled = true;
        private readonly ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();

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


        //public bool IsCommandEnabled
        //{
        //    get => _isCommandEnabled;
        //    private set
        //    {
        //        _isCommandEnabled = value;
        //        OnPropertyChanged();
        //    }
        //}

        public IFileController Controller { get; }


        #region EventHandler

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Controller.HasContentChanged += FileController_HasContentChanged;
            Controller.Message += FileController_Message;
            //Controller.NewFileName += FileController_NewFileName;
            Controller.PropertyChanged += FileController_PropertyChanged;
            Controller.RefreshData += FileController_RefreshData;
            Controller.ShowFileDialog += FileController_ShowFileDialog;
            //Controller.BadFileName += FileController_BadFileName;
            Controller.TranslationError += FileController_TranslationError;
            Controller.UnusedTranslations += FileController_UnusedTranslations;
            Controller.SourceDocumentDeleted += FileController_SourceDocumentDeleted;

            _recentFilesMenu.Initialize(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;

            _ = ProcessCommandLineArgs();
        }

        private void FileController_SourceDocumentDeleted(object? sender, FileSystemEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(() =>
              {
                  MessageBox.Show(
                      this,
                      string.Format(CultureInfo.CurrentCulture, "The XML documentation file {0}\"{1}\"{0} has been deleted!", Environment.NewLine, e.FullPath),
                      App.ProgramName,
                      MessageBoxButton.OK,
                      MessageBoxImage.Warning,
                      MessageBoxResult.OK);
                  ChangeSourceDocument_ExecutedAsync(this, null!);


              }
            , DispatcherPriority.Send);
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!await Controller.CloseCurrentDocument().ConfigureAwait(true))
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



        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void FileController_BadFileName(object? sender, BadFileNameEventArgs e)
        //    => _tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));

        private void FileController_ShowFileDialog(object? sender, ShowFileDialogEventArgs e) =>
            Dispatcher.Invoke(() => e.ShowDialog(this), DispatcherPriority.Send);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FileController_RefreshData(object? sender, EventArgs e) => Dispatcher.Invoke(RefreshData);



        private void FileController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Controller.CurrentDocument))
            {
                _ = Dispatcher.BeginInvoke(() => ShowCurrentDocument());
            }
        }

        private void ShowCurrentDocument()
        {
            IDocument? doc = Controller.CurrentDocument;
            if (doc is null)
            {
                _ccContent.Content = null;
                return;
            }

            if (doc.HasValidSourceDocument)
            {
                doc.PropertyChanged += Doc_PropertyChanged;

                var cntr = new TsltnControl(this, doc);
                _ccContent.Content = cntr;
                _ = cntr._tbOriginal.Focus();

                string? fileName = doc.FileName;

                if (fileName != null)
                {
                    _tasks.Add(_recentFilesMenu.AddRecentFileAsync(fileName));
                }
            }
            else
            {
                _ccContent.Content = null;

                string errorMessage = doc.HasSourceDocument
                                        ? string.Format(
                                          CultureInfo.CurrentCulture,
                                          Res.EmptyOrInvalidFile,
                                          Environment.NewLine, System.IO.Path.GetFileName(doc.SourceDocumentFileName), Res.XmlDocumentationFile)
                                        : string.Format(CultureInfo.CurrentCulture, Res.SourceDocumentNotFound, Environment.NewLine, doc.SourceDocumentFileName);

                ShowErrorMessage(errorMessage);
                ChangeSourceDocument_ExecutedAsync(this, null!);
            }
        }

        private void ShowErrorMessage(string errorMessage)
        {
            _ = MessageBox.Show(this, errorMessage, App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);

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


        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private void FileController_NewFileName(object? sender, NewFileNameEventArgs e)
        //    => _tasks.Add(_recentFilesMenu.AddRecentFileAsync(e.FileName));


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


        private void MiQuit_Click(object sender, RoutedEventArgs e) => Close();


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

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.CurrentDocument != null;
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.CurrentDocument?.Changed ?? false;


        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            new HelpWindow().Show();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            if (Controller.CurrentDocument != null)
            {
                if (!await SaveDocumentAsync(false))
                {
                    return;
                }
            }

            string? xmlFileName = null;
            if(!GetXmlInFileName(ref xmlFileName))
            {
                return;
            }

            try
            {
                await Task.Run(() => Controller.NewDocument(xmlFileName)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, Res.CreationFailed, Environment.NewLine, ex.Message);
                _ = Dispatcher.BeginInvoke(() => ShowErrorMessage(errorMessage), DispatcherPriority.Send);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            if (Controller.CurrentDocument != null)
            {
                if (!await SaveDocumentAsync(false))
                {
                    return;
                }
            }
            _ = Controller.LoadDocumentAsync(null);
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Controller.CurrentDocument is null)
            {
                return;
            }

            if (!await SaveDocumentAsync(false))
            {
                return;
            }

            Controller.CloseCurrentDocument();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = SaveDocumentAsync(false);
        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = SaveDocumentAsync(true);

        private async Task<bool> SaveDocumentAsync(bool showFileDialog)
        {
            IDocument? doc = Controller.CurrentDocument;
            if (doc is null)
            {
                return true;
            }

            RefreshData();

            if(!doc.Changed)
            {
                return true;
            }

            string? fileName = doc.FileName;

            if (fileName is null)
            {
                showFileDialog = true;
                fileName = $"{Path.GetFileNameWithoutExtension(doc.SourceDocumentFileName)}.{doc.TargetLanguage ?? Res.Language}{App.TsltnFileExtension}";
            }

            if (showFileDialog)
            {
                if (!GetTsltnOutFileName())
                {
                    return false;
                }
            }

            var task = Task.Run(() => Controller.CurrentDocument?.Save(fileName));
            _tasks.Add(task);

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, "The file {0}{1}could not be saved:{1}{2}",
                                                    fileName, Environment.NewLine, ex.Message);
                _ = Dispatcher.BeginInvoke(() => ShowErrorMessage(errorMessage), DispatcherPriority.Send);
                return false;
            }

            return true;

            //////////////////////////////////////

            bool GetTsltnOutFileName()
            {
                string? directory = Path.GetDirectoryName(fileName);
                if (string.IsNullOrEmpty(directory))
                {
                    directory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                }

                var dlg = new SaveFileDialog()
                {
                    FileName = Path.GetFileName(fileName),
                    AddExtension = true,
                    CheckFileExists = false,
                    CheckPathExists = true,
                    CreatePrompt = false,
                    Filter = $"{Res.TsltnFile} (*{App.TsltnFileExtension})|*{App.TsltnFileExtension}",
                    InitialDirectory = directory,
                    DefaultExt = App.TsltnFileExtension,
                    DereferenceLinks = true
                };

                if(dlg.ShowDialog(this) == true)
                {
                    fileName = dlg.FileName;
                    return true;
                }

                return false;
            }
        }

        private bool GetXmlInFileName([NotNullWhen(true)]ref string? fileName)
        {
            var dlg = new OpenFileDialog()
            {
                FileName = fileName ?? "",
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = ".xml",
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                DereferenceLinks = true,
                InitialDirectory = System.IO.Path.GetDirectoryName(fileName) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true,
                Title = Res.LoadXmlFile
            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName;
                return true;
            }

            return false;
        }

        private bool GetTsltnInFileName([NotNullWhen(true)] ref string? fileName)
        {
            var dlg = new OpenFileDialog()
            {
                //FileName = fileName,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = App.TsltnFileExtension,
                Filter = $"{Res.TsltnFile} (*{App.TsltnFileExtension})|*{App.TsltnFileExtension}",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName;
                return true;
            }

            return false;
        }


        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {

            await Controller.TranslateAsync().ConfigureAwait(true);
        }

        private void FileController_TranslationError(object? sender, DataErrorEventArgs e) => TranslationError?.Invoke(this, e);

        private void FileController_UnusedTranslations(object? sender, UnusedTranslationEventArgs e)
        {
            var wnd = new SelectUnusedTranslationsWindow(System.IO.Path.GetFileName(Controller.CurrentDocument!.FileName), e.UnusedTranslations);

            if (true == wnd.ShowDialog(this))
            {
                foreach (UnusedTranslationUserControl cntr in wnd.Controls)
                {
                    if (cntr.Remove)
                    {
                        e.TranslationsToRemove.Add(cntr.Kvp.Key);
                    }
                }
            }
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
                InitialDirectory = Path.GetDirectoryName(sourceFilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true,
                Title = Res.NewSourceFile

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };


            if (dlg.ShowDialog(this) == true)
            {
                await Controller.ChangeSourceDocumentAsync(dlg.FileName).ConfigureAwait(false);
            }
        }

        #endregion


        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));



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
                await Controller.OpenTsltnFromCommandLineAsync(args[1]).ConfigureAwait(true);
            }
        }


    }
}
