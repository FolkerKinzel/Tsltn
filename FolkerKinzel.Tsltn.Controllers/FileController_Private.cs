using FolkerKinzel.Tsltn.Controllers.Enums;
using FolkerKinzel.Tsltn.Controllers.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            var args = new ShowFileDialogEventArgs(DlgType.SaveFileDialog)
            {
                FileName = Path.GetFileName(fileName),
                AddExtension = true,
                CheckFileExists = false,
                CheckPathExists = true,
                CreatePrompt = false,
                Filter = $"{Res.TsltnFile} (*{TsltnFileExtension})|*{TsltnFileExtension}",
                InitialDirectory = _doc.TsltnFileName != null ? Path.GetDirectoryName(_doc.TsltnFileName) ?? "" : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
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
            fileName = _doc.SourceDocumentFileName;

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
            if (fileName is null)
            {
                fileName = _doc.TsltnFileName ?? this.FileName;

                if (!GetTsltnOutFileName(ref fileName))
                {
                    return false;
                }
            }
            
            OnRefreshData();

            await _doc.WaitAllTasks().ConfigureAwait(false);

            try
            {
                var task = Task.Run(() => _doc.SaveTsltnAs(fileName));
                _doc.Tasks.Add(task);
                await task.ConfigureAwait(false);

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
            if (_watcher.WatchedFile != _doc.SourceDocumentFileName)
            {
                _doc.ChangeSourceDocumentFileName(_watcher.WatchedFile);
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
            OnRefreshData();

            if ((_doc.TsltnFileName != null && !_doc.Changed) || await SaveTsltnAsync().ConfigureAwait(false))
            {

                OnHasContentChanged(false);

                string? fileName = _doc.TsltnFileName;
                _doc.CloseTsltn();

                await OpenTsltnAsync(fileName).ConfigureAwait(false);
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
