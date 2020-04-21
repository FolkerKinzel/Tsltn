using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        private readonly IRecentFilesMenu _recentFilesMenu;

        private Task? _currentTask;


        public MainWindow(IDocument doc, IRecentFilesMenu recentFilesMenu)
        {
            this._doc = doc;
            InitializeComponent();

            this._recentFilesMenu = recentFilesMenu;
        }


        public string FileName
        {
            get
            {
                string? filename = _doc.TsltnFileName;

                if (filename is null && _ccContent?.Content is TsltnControl cntr)
                {
                    cntr.UpdateSource();
                    return $"{System.IO.Path.GetFileNameWithoutExtension(_doc.SourceDocumentFileName)}.{cntr.TargetLanguage ?? "<Language>"}{App.TSLTN_FILE_EXTENSION}";
                }

                return filename ?? "";
            }
        }


        public ObservableCollection<DataError> Errors { get; } = new ObservableCollection<DataError>();


        #region EventHandler

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _recentFilesMenu.SetRecentFilesMenuItem(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;
        }

        private async void Window_Closing(object sender, CancelEventArgs e)
        {
            if(!await DoCloseTsltnAsync().ConfigureAwait(false))
            {
                e.Cancel = true;
            }
        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                _currentTask?.Wait();
            }
            catch { }
            
        }

        private async void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            if (System.IO.File.Exists(e.FileName))
            {
                await DoOpenAsync(e.FileName).ConfigureAwait(false);

                if(FileName != null)
                {
                    _currentTask = _recentFilesMenu.AddRecentFileAsync(e.FileName);
                }
            }
            else
            {
                _currentTask = _recentFilesMenu.RemoveRecentFileAsync(e.FileName);
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
        private async void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            await DoCloseTsltnAsync().ConfigureAwait(true);

            string? xmlFileName = null;

            if (GetXmlInFileName(ref xmlFileName))
            {
                try
                {
                    await Task.Run(() => _doc.NewTsltn(xmlFileName)).ConfigureAwait(true);

                    if (_doc.FirstNode is null)
                    {
                        MessageBox.Show(
                            string.Format(CultureInfo.CurrentCulture, Res.EmptyOrInvalidFile, Environment.NewLine, xmlFileName, Res.XmlDocumentationFile),
                            App.PROGRAM_NAME,
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK);

                        _doc.Close();
                    }
                    else
                    {
                        _ccContent.Content = new TsltnControl(this, _doc);
                    }

                }
                catch (AggregateException ex)
                {
                    MessageBox.Show(this, ex.Message, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                finally
                {
                    OnPropertyChanged(nameof(FileName));
                }


            }

        }

        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            _ = DoOpenAsync(null);

        }


        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _doc.SourceDocumentFileName != null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Close_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            await DoCloseTsltnAsync().ConfigureAwait(false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e) => _ = DoSaveAsync(_doc.TsltnFileName);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = DoSaveAsync(null);


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            if(!await DoSaveAsync(_doc.TsltnFileName).ConfigureAwait(true))
            {
                return;
            }

            if(GetXmlOutFileName(out string? fileName))
            {
                (List<DataError> Errors, List<KeyValuePair<long, string>> UnusedTranslations) result;

                try
                {
                    result = await DoTranslateAsync(fileName).ConfigureAwait(true);
                }
                catch (AggregateException ex)
                {
                    MessageBox.Show(this, ex.Message, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                    try
                    {
                        await Task.Run(() => _doc.ReloadSourceDocument(_doc.SourceDocumentFileName!)).ConfigureAwait(false);
                    }
                    catch
                    {
                        _ccContent.Content = null;
                        _doc.Close();
                        OnPropertyChanged(nameof(FileName));
                    }

                    return;
                }//catch


                if (result.Errors.Count != 0)
                {
                    this.Errors.Clear();

                    foreach (var error in result.Errors)
                    {
                        Errors.Add(error);
                    }
                }

                if (result.UnusedTranslations.Count != 0)
                {
                    RemoveUnusedTranslations(result.UnusedTranslations);
                }

            }//if  
        }

        

       


        #endregion


        #region private

        private bool GetXmlInFileName([NotNullWhen(true)] ref string? fileName)
        {
            var dlg = new OpenFileDialog()
            {
                FileName = fileName,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = ".xml",
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true,
                Title = Res.OpenSourceFile

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName ?? "";
                return true;
            }

            return false;
        }

        private bool GetTsltnInFileName([NotNullWhen(true)] ref string? fileName)
        {
            var dlg = new OpenFileDialog()
            {
                FileName = fileName,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = App.TSLTN_FILE_EXTENSION,
                Filter = $"{Res.TsltnFile} (*{App.TSLTN_FILE_EXTENSION})|*{App.TSLTN_FILE_EXTENSION}",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName ?? "";
                return true;
            }

            return false;
        }

        private bool GetTsltnOutFileName([NotNullWhen(true)] ref string? fileName)
        {
            var dlg = new SaveFileDialog()
            {
                FileName = fileName,
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.TsltnFile} (*{App.TSLTN_FILE_EXTENSION})|*{App.TSLTN_FILE_EXTENSION}",
                InitialDirectory = _doc.TsltnFileName != null ? System.IO.Path.GetDirectoryName(_doc.TsltnFileName) : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = App.TSLTN_FILE_EXTENSION,
                DereferenceLinks = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName ?? "";
                return true;
            }
            else
            {
                fileName = null;
                return false;
            }
        }


        private bool GetXmlOutFileName([NotNullWhen(true)] out string? fileName)
        {
            fileName = _doc.SourceDocumentFileName;

            var dlg = new SaveFileDialog()
            {
                FileName = fileName,
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                InitialDirectory = System.IO.Path.GetDirectoryName(_doc.SourceDocumentFileName),
                DefaultExt = ".xml",
                DereferenceLinks = true
            };

            if (dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName ?? "";
                return true;
            }
            else
            {
                fileName = null;
                return false;
            }
        }

        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private async Task<bool> DoCloseTsltnAsync()
        {
            if (this._ccContent.Content is TsltnControl cntr)
            {
                cntr.UpdateSource();
            }
            else
            {
                return true;
            }

            if (_doc.Changed)
            {
                var res = MessageBox.Show(
                    string.Format(CultureInfo.CurrentCulture, Res.FileWasChanged, System.IO.Path.GetFileName(this.FileName), Environment.NewLine),
                    App.PROGRAM_NAME,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                switch (res)
                {
                    case MessageBoxResult.Yes:
                        {
                            await DoSaveAsync(FileName).ConfigureAwait(true);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return false;
                }
            }

            _ccContent.Content = null;
            _doc.Close();
            OnPropertyChanged(nameof(FileName));

            return true;
        }



        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private async Task<bool> DoSaveAsync(string? fileName)
        {
            if (fileName is null || _doc.TsltnFileName is null)
            {
                fileName = _doc.TsltnFileName ?? this.FileName;
                if (!GetTsltnOutFileName(ref fileName))
                {
                    return false;
                }
            }

            if(this._ccContent.Content is TsltnControl cntr)
            {
                cntr.UpdateSource();
            }

            try
            {
                _currentTask = Task.Run(() => _doc.SaveTsltnAs(fileName));
                await _currentTask.ConfigureAwait(false);

                _currentTask = _recentFilesMenu.AddRecentFileAsync(FileName);
            }
            catch (AggregateException e)
            {
                MessageBox.Show(this, e.Message, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                OnPropertyChanged(nameof(FileName));

                return false;
            }

            OnPropertyChanged(nameof(FileName));
            return true;
        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        private async Task DoOpenAsync(string? fileName)
        {
            if(StringComparer.OrdinalIgnoreCase.Equals(fileName, this.FileName))
            {
                MessageBox.Show(this, Res.FileAlreadyOpen, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK);
                return;
            }

            if (this._ccContent.Content is TsltnControl cntr)
            {
                cntr.UpdateSource();

                if (_doc.Changed)
                {
                    var res = MessageBox.Show(this,
                        string.Format(CultureInfo.CurrentCulture, Res.FileWasChanged, System.IO.Path.GetFileName(this.FileName), Environment.NewLine),
                        App.PROGRAM_NAME,
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            {
                                await DoSaveAsync(FileName).ConfigureAwait(true);
                            }
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            return;
                    }
                }
            }

            if (fileName is null)
            {
                if (!GetTsltnInFileName(ref fileName))
                {
                    return;
                }
            }

            try
            {
                if (!await Task.Run(() => _doc.Open(fileName)).ConfigureAwait(true))
                {
                    _ccContent.Content = null;
                    OnPropertyChanged(nameof(FileName));

                    MessageBox.Show(this,
                            string.Format(CultureInfo.CurrentCulture, Res.SourceDocumentNotFound, Environment.NewLine, _doc.SourceDocumentFileName),
                            App.PROGRAM_NAME,
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK);


                    string? xmlFileName = null;
                    try
                    {
                        xmlFileName = System.IO.Path.GetFileName(_doc.SourceDocumentFileName);
                    }
                    catch { }

                    do
                    {
                        if (!GetXmlInFileName(ref xmlFileName))
                        {
                            _doc.Close();
                            return;
                        }


                    } while (!_doc.ReloadSourceDocument(xmlFileName));
                }


                if (_doc.FirstNode is null)
                {
                    _ccContent.Content = null;

                    MessageBox.Show(this,
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Res.EmptyOrInvalidFile,
                            Environment.NewLine, System.IO.Path.GetFileName(_doc.SourceDocumentFileName), Res.XmlDocumentationFile),
                        App.PROGRAM_NAME,
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK);
                }
                else
                {
                    _ccContent.Content = new TsltnControl(this, _doc);
                }

                OnPropertyChanged(nameof(FileName));

                _currentTask = _recentFilesMenu.AddRecentFileAsync(FileName);
            }
            catch (AggregateException e)
            {
                MessageBox.Show(this, e.Message, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                _ccContent.Content = null;
                _doc.Close();
                OnPropertyChanged(nameof(FileName));
            }
        }


        private Task<(List<DataError> Errors, List<KeyValuePair<long, string>> UnusedTranslations)> DoTranslateAsync(string fileName)
        {
            return Task.Run(() =>
            {
                _doc.Translate(fileName, Res.InvalidXml, out List<DataError> errors, out List<KeyValuePair<long, string>> unusedTranslations);
                return (Errors: errors, UnusedTranslations: unusedTranslations);
            });
        }


        private void RemoveUnusedTranslations(List<KeyValuePair<long, string>> unusedTranslations)
        {

        }


        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

        
    }
}
