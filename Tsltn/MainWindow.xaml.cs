using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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


        public MainWindow(IDocument doc)
        {
            this._doc = doc;
            InitializeComponent();
        }

        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        public string? FileName => _doc.TsltnFileName;


        


        private void Help_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string? xmlFileName = null;

            if (GetXmlInFileName(ref xmlFileName))
            {
                try
                {
                    _doc.NewTsltn(xmlFileName);

                    if(_doc.FirstNode is null)
                    {
                        MessageBox.Show(
                            string.Format(Res.NotAXmlFile, xmlFileName, Environment.NewLine, Res.XmlDocumentationFile),
                            App.PROGRAM_NAME,
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK);

                        _doc.Close();
                    }
                    else
                    {
                        frmContent.Content = new TsltnPage(this, _doc.FirstNode, _doc.SourceLanguage, _doc.TargetLanguage);
                    }

                }
                catch(Exception ex)
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

        }

        private async void Close_Executed(object sender, ExecutedRoutedEventArgs? e)
        {
            if (!(frmContent.Content is TsltnPage page))
            {
                return;
            }

            page.SetTranslation();

            if (page.Changed)
            {
                var res = MessageBox.Show(
                    string.Format("The file \"{0}\" was changed.{1}Do you want to save the changes?", System.IO.Path.GetFileName(this.FileName), Environment.NewLine),
                    App.PROGRAM_NAME,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                switch (res)
                {
                    case MessageBoxResult.Yes:
                        await DoSaveAsync(FileName);
                        page.Changed = false;
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return;     
                }

                if(page.Changed)
                {
                    return;
                }

                frmContent.Content = null;

                _doc.Close();

                OnPropertyChanged(nameof(FileName));
            }
        }

        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = frmContent.Content != null;
        }

        private async void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs? e)
        {
            await DoSaveAsync(FileName).ConfigureAwait(false);

            if (frmContent.Content is TsltnPage page)
            {
                page.Changed = false;
            }
        }

        private async void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            string? fileName = this.FileName;
            if(GetTsltnOutFileName(ref fileName))
            {
                await DoSaveAsync(fileName).ConfigureAwait(false);

                if (frmContent.Content is TsltnPage page)
                {
                    page.Changed = false;
                }
            }
        }

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
                ReadOnlyChecked = true,
                DefaultExt = ".xml",
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true,
                ShowReadOnly = true

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
                CheckFileExists = true,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.TsltnFile} (*.tsltn)|*.tsltn",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = ".tsltn",
                DereferenceLinks = true
            };

            if(dlg.ShowDialog(this) == true)
            {
                fileName = dlg.FileName ?? "";
                return true;
            }

            return false;
        }


        private Task DoSaveAsync(string? fileName)
        {
            if(fileName != null || GetTsltnOutFileName(ref fileName))
            {
                return Task.Run(() => _doc.SaveTsltnAs(fileName));

            }

            return Task.CompletedTask;

            
        }

        private void miQuit_Click(object sender, RoutedEventArgs e)
        {

            
            this.Close();
        }

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

            MessageBox.Show(sb.ToString(), App.PROGRAM_NAME + " - Info", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        
    }
}
