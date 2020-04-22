using FolkerKinzel.Tsltn.Controllers.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace FolkerKinzel.Tsltn.Controllers
{
    public partial class FileController : INotifyPropertyChanged, IFileController
    {
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

            var args = new ShowFileDialogEventArgs(dlg);

            this.OnFileDialog(args);

            if (args.Result == true)
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

                DefaultExt = TSLTN_FILE_EXTENSION,
                Filter = $"{Res.TsltnFile} (*{TSLTN_FILE_EXTENSION})|*{TSLTN_FILE_EXTENSION}",
                DereferenceLinks = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                Multiselect = false,
                ValidateNames = true

                //ReadOnlyChecked = true,
                //ShowReadOnly = true

            };

            var args = new ShowFileDialogEventArgs(dlg);

            OnFileDialog(args);

            if (args.Result == true)
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
                Filter = $"{Res.TsltnFile} (*{TSLTN_FILE_EXTENSION})|*{TSLTN_FILE_EXTENSION}",
                InitialDirectory = _doc.TsltnFileName != null ? System.IO.Path.GetDirectoryName(_doc.TsltnFileName) : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = TSLTN_FILE_EXTENSION,
                DereferenceLinks = true
            };

            var args = new ShowFileDialogEventArgs(dlg);

            OnFileDialog(args);

            if (args.Result == true)
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

            var args = new ShowFileDialogEventArgs(dlg);

            OnFileDialog(args);

            if (args.Result == true)
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

    }
}
