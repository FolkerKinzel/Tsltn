using FolkerKinzel.Tsltn.Controllers.Enums;
using Microsoft.Win32;
using System;
using System.Windows;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class ShowFileDialogEventArgs : EventArgs
    {
        public ShowFileDialogEventArgs(DlgType dialogType)
        {
            this.DialogType = dialogType;
        }


        public bool? Result { get; private set; }


        public DlgType DialogType { get; }

        public string FileName { get; internal set; } = string.Empty;
        public bool AddExtension { get; internal set; } = true;
        public bool CheckFileExists { get; internal set; } = true;
        public bool CheckPathExists { get; internal set; } = true;
        public string DefaultExt { get; internal set; } = string.Empty;
        public string Filter { get; internal set; } = string.Empty;
        public bool DereferenceLinks { get; internal set; } = true;
        public string InitialDirectory { get; internal set; } = string.Empty;
        public bool Multiselect { get; internal set; }
        public bool ValidateNames { get; internal set; } = true;
        public string Title { get; internal set; } = string.Empty;
        public bool CreatePrompt { get; internal set; }


        public void ShowDialog(Window owner)
        {

            FileDialog dlg = DialogType == DlgType.OpenFileDialog
            ? (FileDialog)new OpenFileDialog()
            {
                FileName = this.FileName,
                AddExtension = this.AddExtension,
                CheckFileExists = this.CheckFileExists,
                CheckPathExists = this.CheckPathExists,

                DefaultExt = this.DefaultExt,
                Filter = this.Filter,
                DereferenceLinks = this.DereferenceLinks,
                InitialDirectory = this.InitialDirectory,
                Multiselect = this.Multiselect,
                ValidateNames = this.ValidateNames,
                Title = this.Title
            }

            : new SaveFileDialog()
            {
                FileName = this.FileName,
                AddExtension = this.AddExtension,
                CheckFileExists = this.CheckFileExists,
                CheckPathExists = this.CheckPathExists,
                CreatePrompt = this.CreatePrompt,
                Filter = this.Filter,
                InitialDirectory = this.InitialDirectory,
                DefaultExt = this.DefaultExt,
                DereferenceLinks = this.DereferenceLinks
            };

            this.Result = dlg.ShowDialog(owner);
            this.FileName = dlg.FileName;
        }
    }
}