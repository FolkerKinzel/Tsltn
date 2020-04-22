using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
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



        public MainWindow(IDocument doc, IFileController fileController, IRecentFilesMenu recentFilesMenu)
        {
            this._doc = doc;
            this._fileController = fileController;
            InitializeComponent();

            this._recentFilesMenu = recentFilesMenu;
        }


        public string FileName => _fileController.FileName;
        



        public ObservableCollection<DataError> Errors { get; } = new ObservableCollection<DataError>();
        //{ 
        //    new DataError(ErrorLevel.Error, "Das ist ein Fehler.", null!),
        //    new DataError(ErrorLevel.Warning, "Das ist eine Warnung. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.", null!),
        //    new DataError(ErrorLevel.Information, "Das ist eine Information.", null!),

        //};


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


            _recentFilesMenu.InitializeAsync(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;
        }

        private void _fileController_BadFileName(object? sender, BadFileNameEventArgs e)
        {
            _fileController.Tasks.Add(_recentFilesMenu.RemoveRecentFileAsync(e.FileName));
        }

        private void _fileController_ShowFileDialog(object? sender, ShowFileDialogEventArgs e)
        {
            e.Result = e.Dialog.ShowDialog(this);
        }

        private void _fileController_RefreshData(object? sender, EventArgs e)
        {
            if(_ccContent.Content is TsltnControl control)
            {
                control.UpdateSource();
            }
        }

        private void _fileController_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(_fileController.FileName))
            {
                OnPropertyChanged(nameof(FileName));
            }
        }

        private void _fileController_NewFileName(object? sender, NewFileNameEventArgs e)
        {
            _fileController.Tasks.Add(_recentFilesMenu.AddRecentFileAsync(e.FileName));
        }

        private void _fileController_HasContentChanged(object? sender, HasContentChangedEventArgs e)
        {
            if(e.HasContent)
            {
                _ccContent.Content = new TsltnControl(this, _doc);
            }
            else
            {
                _ccContent.Content = null;
            }
        }

        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void _fileController_Message(object? sender, MessageEventArgs e)
        {
            e.Result = MessageBox.Show(this, e.Message, App.PROGRAM_NAME, e.Button, e.Image, e.DefaultResult);
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if(!await _fileController.DoCloseTsltnAsync().ConfigureAwait(true))
            {
                e.Cancel = true;
            }
        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                await Task.WhenAll(_fileController.Tasks.ToArray()).ConfigureAwait(false);
            }
            catch { }
        }

        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                _ = _fileController.DoOpenAsync(e.FileName);
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

        #endregion

        #region Commands

        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            _= _fileController.DoNewTsltnAsync();
        }

        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            _ = _fileController.DoOpenAsync(null);

        }


        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _doc.SourceDocumentFileName != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Close_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            await _fileController.DoCloseTsltnAsync().ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e) => _ = _fileController.DoSaveAsync(_doc.TsltnFileName);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = _fileController.DoSaveAsync(null);


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            var result = await _fileController.TranslateAsync().ConfigureAwait(true);

            this.Errors.Clear();

            foreach (var error in result.Errors)
            {
                Errors.Add(error);
            }
            

            if (result.UnusedTranslations.Any())
            {
                RemoveUnusedTranslations(result.UnusedTranslations);
            }
        }

        private void RemoveUnusedTranslations(IEnumerable<KeyValuePair<long, string>> unusedTranslations)
        {

        }






        #endregion





        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


    }
}
