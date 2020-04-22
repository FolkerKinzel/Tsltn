using Microsoft.Win32;
using System;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class ShowFileDialogEventArgs : EventArgs
    {
        public ShowFileDialogEventArgs(FileDialog dialog)
        {
            this.Dialog = dialog;
        }

        public FileDialog Dialog { get; }

        public bool? Result { get; set; }
    }
}