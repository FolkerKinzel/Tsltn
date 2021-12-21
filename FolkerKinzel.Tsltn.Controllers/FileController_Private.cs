using FolkerKinzel.Tsltn.Controllers.Enums;
using FolkerKinzel.Tsltn.Controllers.Resources;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolkerKinzel.Tsltn.Controllers
{
    public sealed partial class FileController : INotifyPropertyChanged, IFileController
    {
        private bool GetXmlInFileName([NotNullWhen(true)] ref string? fileName)
        {
            var args = new ShowFileDialogEventArgs(DlgType.OpenFileDialog)
            {
                FileName = fileName ?? "",
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
            };

            this.OnFileDialog(args);

            if (args.Result == true)
            {
                fileName = args.FileName;
                return true;
            }

            return false;
        }



        private bool GetTsltnInFileName([NotNullWhen(true)] out string? fileName)
        {
            var args = new ShowFileDialogEventArgs(DlgType.OpenFileDialog)
            {
                //FileName = fileName,
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = TsltnFileExtension,
                Filter = $"{Res.TsltnFile} (*{TsltnFileExtension})|*{TsltnFileExtension}",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true
            };

            OnFileDialog(args);

            if (args.Result == true)
            {
                fileName = args.FileName;
                return true;
            }
            else
            {
                fileName = null;
                return false;
            }
        }

        private bool GetTsltnOutFileName(ref string fileName)
        {
            if (FileName.Length == 0 && CurrentDocument?.SourceDocumentFileName != null)
            {
                OnRefreshData();
                fileName = $"{Path.GetFileNameWithoutExtension(CurrentDocument.SourceDocumentFileName)}.{CurrentDocument.TargetLanguage ?? Res.Language}{TsltnFileExtension}";
            }

            var args = new ShowFileDialogEventArgs(DlgType.SaveFileDialog)
            {
                FileName = Path.GetFileName(fileName),
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.TsltnFile} (*{TsltnFileExtension})|*{TsltnFileExtension}",
                InitialDirectory = FileName.Length != 0 ? Path.GetDirectoryName(FileName) ?? "" : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = TsltnFileExtension,
                DereferenceLinks = true
            };

            OnFileDialog(args);

            if (args.Result == true)
            {
                fileName = args.FileName;
                return true;
            }
            else
            {
                // Da bei der Rückgabe von false nichts gespeichert wird, kann der Rückgabewert leer sein.
                fileName = "";
                return false;
            }
        }


        private bool GetXmlOutFileName([NotNullWhen(true)] out string? fileName)
        {
            fileName = CurrentDocument?.SourceDocumentFileName;

            var args = new ShowFileDialogEventArgs(DlgType.SaveFileDialog)
            {
                Title = Res.SaveTranslationAs,
                FileName = Path.GetFileName(fileName) ?? "",
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.XmlDocumentationFile} (*.xml)|*.xml",
                InitialDirectory = System.IO.Path.GetDirectoryName(fileName) ?? "",
                DefaultExt = ".xml",
                DereferenceLinks = true
            };

            OnFileDialog(args);

            if (args.Result == true)
            {
                fileName = args.FileName;
                return true;
            }
            else
            {
                fileName = null;
                return false;
            }
        }




        private async Task<bool> DoSaveTsltnAsync(string? fileName)
        {
            Debug.Assert(_doc != null);

            if (fileName is null)
            {
                fileName = FileName;

                if (!GetTsltnOutFileName(ref fileName))
                {
                    return false;
                }
            }
            
            OnRefreshData();

            //await _doc.WaitAllTasks().ConfigureAwait(false);

            try
            {
                await Task.Run(() => _doc.Save(fileName)).ConfigureAwait(false);
                OnNewFileName(FileName);
            }
            catch (Exception e)
            {
                OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                OnPropertyChanged(nameof(FileName));

                return false;
            }

            OnPropertyChanged(nameof(FileName));
            return true;
        }

        private async void FileWatcher_Reload(object? sender, EventArgs e)
        {
            if (_watcher.WatchedFile != CurrentDocument?.SourceDocumentFileName)
            {
                if (_doc != null)
                {
                    _doc.SourceDocumentFileName = _watcher.WatchedFile;
                }
            }

            var args = new MessageEventArgs(
                string.Format(CultureInfo.InvariantCulture, Res.SourceDocumentChanged, Environment.NewLine),
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            OnMessage(args);

            if (args.Result == MessageBoxResult.Yes)
            {
                await ReloadTsltnAsync().ConfigureAwait(false);
            }
        }



        private async Task ReloadTsltnAsync()
        {
            IFileAccess? doc = CurrentDocument;
            if(doc is null)
            {
                return;
            }

            OnRefreshData();

            if ((doc.FileName.Length != 0 && !doc.Changed) || await SaveDocumentAsync().ConfigureAwait(false))
            {

                string? fileName = doc.FileName;

                _ = await CloseTsltnAsync(false).ConfigureAwait(false);
                await LoadDocumentAsync(fileName).ConfigureAwait(false);
            }
            else
            {
                var args = new MessageEventArgs(
                    Res.NotReloaded,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK);

                OnMessage(args);
            }
        }


    }
}
