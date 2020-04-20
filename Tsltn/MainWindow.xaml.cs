using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

                if (filename is null && _ccContent?.Content != null)
                {
                    return $"{Res.DefaultFileName}{App.TSLTN_FILE_EXTENSION}";
                }

                return filename ?? "";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _recentFilesMenu.SetRecentFilesMenuItem(miRecentFiles);
            _recentFilesMenu.RecentFileSelected += RecentFilesMenu_RecentFileSelected;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Close_Executed(this, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _currentTask?.Wait();
        }

        private void RecentFilesMenu_RecentFileSelected(object? sender, RecentFileSelectedEventArgs e)
        {
            throw new NotImplementedException();
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


        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private async void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            Close_Executed(this, null);

            string? xmlFileName = null;

            if (GetXmlInFileName(ref xmlFileName))
            {
                try
                {
                    await Task.Run(() => _doc.NewTsltn(xmlFileName)).ConfigureAwait(true);

                    if (_doc.FirstNode is null)
                    {
                        MessageBox.Show(
                            string.Format(CultureInfo.CurrentCulture, Res.NotAXmlFile, xmlFileName, Environment.NewLine, Res.XmlDocumentationFile),
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
            Close_Executed(this, null);

        }


        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _ccContent.Content != null;
        }


        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private async void Close_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            if (this._ccContent.Content is TsltnControl cntr)
            {
                cntr.UpdateSource();
            }
            else
            {
                return;
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
                        return;
                }



                _ccContent.Content = null;

                _doc.Close();

                OnPropertyChanged(nameof(FileName));
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e) => _ = DoSaveAsync(_doc.TsltnFileName);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = DoSaveAsync(null);


        private void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {

        }


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
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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



        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private async Task DoSaveAsync(string? fileName)
        {
            if (fileName is null || _doc.TsltnFileName is null)
            {
                fileName = _doc.TsltnFileName ?? this.FileName;
                if (!GetTsltnOutFileName(ref fileName))
                {
                    return;
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
                _recentFilesMenu.AddRecentFileAsync(FileName);
            }
            catch (AggregateException e)
            {
                _ = Dispatcher.BeginInvoke(new Action(
                    () => MessageBox.Show(this, e.Message, App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK)));
            }

            OnPropertyChanged(nameof(FileName));
            
        }

        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


    }
}
