using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
using FolkerKinzel.WpfTools;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

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
        }


        private readonly DataError MissingTranslationWarning = new DataError(ErrorLevel.Warning, Res.UntranslatedElement, null);
        private readonly DataError InvalidSourceLanguage = new DataError(ErrorLevel.Error, Res.InvalidSourceLanguage, null);
        private readonly DataError InvalidTargetLanguage = new DataError(ErrorLevel.Error, Res.InvalidTargetLanguage, null);


        public string FileName => _fileController.FileName;


        public bool IsCommandEnabled
        {
            get => _isCommandEnabled;
            set
            {
                _isCommandEnabled = value;
                OnPropertyChanged(nameof(IsCommandEnabled));
            }
        }


        public ObservableCollection<DataError> Errors { get; } = new ObservableCollection<DataError>();


        //{ 
        //    new DataError(ErrorLevel.Error, "Das ist ein Fehler.", null!),
        //    new DataError(ErrorLevel.Warning, "Das ist eine Warnung. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.", null!),
        //    new DataError(ErrorLevel.Information, "Das ist eine Information.", null!),

        //};


        #region EventHandler

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _fileController.HasContentChanged += _fileController_HasContentChanged;
            _fileController.Message += _fileController_Message;
            _fileController.NewFileName += _fileController_NewFileName;
            _fileController.PropertyChanged += _fileController_PropertyChanged;
            _fileController.RefreshData += _fileController_RefreshData;
            _fileController.ShowFileDialog += _fileController_ShowFileDialog;
            _fileController.BadFileName += _fileController_BadFileName;

            await _recentFilesMenu.InitializeAsync(miRecentFiles).ConfigureAwait(false);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;

            await ProcessCommandLineArgs().ConfigureAwait(false);

        }

        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _fileController_BadFileName(object? sender, BadFileNameEventArgs e)
        {
            _fileController.Tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
        }

        private void _fileController_ShowFileDialog(object? sender, ShowFileDialogEventArgs e)
        {
            e.Result = e.Dialog.ShowDialog(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _fileController_RefreshData(object? sender, EventArgs e) => RefreshData();



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
            _fileController.Tasks.Add(_recentFilesMenu.AddRecentFileAsync(e.FileName));
        }


        private void _fileController_HasContentChanged(object? sender, HasContentChangedEventArgs e)
        {
            if (e.HasContent)
            {
                var cntr = new TsltnControl(this, _doc);
                _ccContent.Content = cntr;
                cntr.PropertyChanged += TsltnControl_PropertyChanged;
                cntr.LanguageErrorChanged += TsltnControl_LanguageErrorChanged;
                _fileController.Tasks.Add(CheckUntranslatedNodesAsync(cntr));
            }
            else
            {
                _ccContent.Content = null;
                Errors.Clear();
            }
        }

        
        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void _fileController_Message(object? sender, MessageEventArgs e)
        {
            e.Result = MessageBox.Show(this, e.Message, App.PROGRAM_NAME, e.Button, e.Image, e.DefaultResult);
        }

        private void TsltnControl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TsltnControl.HasTranslation))
            {
                if (sender is TsltnControl cntr)
                {
                    _fileController.Tasks.Add(CheckUntranslatedNodesAsync(cntr));
                }
            }
        }

        private void TsltnControl_LanguageErrorChanged(object? sender, System.Windows.Controls.ValidationErrorEventArgs e)
        {
            if(e.OriginalSource is TextBox tb)
            {
                if (_ccContent.Content is TsltnControl cntr)
                {
                    if (tb.Name == nameof(TsltnControl._tbSourceLanguage))
                    {
                        this.Errors.Remove(InvalidSourceLanguage);

                        if (Validation.GetHasError(cntr._tbSourceLanguage))
                        {
                            this.Errors.Insert(0, InvalidSourceLanguage);
                        }
                    }
                    else
                    {
                        this.Errors.Remove(InvalidTargetLanguage);

                        if (Validation.GetHasError(cntr._tbTargetLanguage))
                        {
                            this.Errors.Insert(0, InvalidTargetLanguage);
                        }
                    }
                }
            }
        }

        private void DataError_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataError? err = (e.OriginalSource as FrameworkElement)?.DataContext as DataError;

            if (_ccContent.Content is TsltnControl control)
            {
                control.Navigate(err?.Node);
            }
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
            await WaitAllTasks().ConfigureAwait(false);
        }

        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                _ = _fileController.OpenTsltnAsync(e.FileName);
            }
            else
            {
                _fileController.Tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
            }
        }


        private void miQuit_Click(object sender, RoutedEventArgs e) => this.Close();


        private void Info_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(64);
            sb.Append(App.PROGRAM_NAME);
            sb.Append(Environment.NewLine);
            sb.Append("Version: ");
            sb.Append(((AssemblyFileVersionAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyFileVersionAttribute)))?.Version);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(((AssemblyCopyrightAttribute?)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute)))?.Copyright);

            MessageBox.Show(sb.ToString(), $"{App.PROGRAM_NAME} - {Res.About}", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _miSave_Loaded(object sender, RoutedEventArgs e) => RefreshData();

        #endregion

        #region Commands

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            new HelpWindow().Show();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            _ = _fileController.NewTsltnAsync();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = _fileController.OpenTsltnAsync(null);


        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && _doc.SourceDocumentFileName != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Close_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e) => _ = _fileController.CloseTsltnAsync();

        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCommandEnabled && _doc.Changed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e)
        {
            IsCommandEnabled = false;
            await _fileController.SaveTsltnAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            IsCommandEnabled = false;
            await WaitAllTasks().ConfigureAwait(true);
            await _fileController.SaveAsTsltnAsync().ConfigureAwait(false);
            IsCommandEnabled = true;
        }

        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            IsCommandEnabled = false;

            await WaitAllTasks().ConfigureAwait(true);

            var result = await _fileController.TranslateAsync().ConfigureAwait(true);

            IsCommandEnabled = true;

            this.Errors.Clear();

            if (this._ccContent.Content is TsltnControl tsltnControl)
            {
                foreach (var error in result.Errors)
                {
                    Errors.Add(error);
                }

                await CheckUntranslatedNodesAsync(tsltnControl).ConfigureAwait(true);


                if (result.UnusedTranslations.Any())
                {
                    var wnd = new SelectUnusedTranslationsWindow(System.IO.Path.GetFileName(_fileController.FileName), result.UnusedTranslations);

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
            };
        }


        #endregion


        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        private async Task CheckUntranslatedNodesAsync(TsltnControl cntr)
        {
            IsCommandEnabled = false;

            await WaitAllTasks().ConfigureAwait(true);

            var task = Task.Run(cntr.CurrentNode.GetNextUntranslated);
            _fileController.Tasks.Add(task);
            INode? result = await task.ConfigureAwait(true);

            IsCommandEnabled = true;

            if (_doc.SourceDocumentFileName != null)
            {
                var err = MissingTranslationWarning;
                err.Node = result;

                if (result is null)
                {
                    Errors.Remove(err);
                }
                else
                {
                    if (!Errors.Contains(err))
                    {
                        Errors.Insert(0, err);
                    }
                }
            }
        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async Task WaitAllTasks()
        {
            try
            {
                await Task.WhenAll(_fileController.Tasks.ToArray()).ConfigureAwait(false);
            }
            catch { }

            _fileController.Tasks.Clear();
        }


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
                try
                {
                    string fileName = System.IO.Path.GetFullPath(args[1]);

                    if (System.IO.File.Exists(fileName))
                    {
                        IsCommandEnabled = false;
                        await _fileController.OpenTsltnAsync(fileName).ConfigureAwait(false);
                    }

                }
                catch { }
                finally { IsCommandEnabled = true; }
            }
        }

    }
}
