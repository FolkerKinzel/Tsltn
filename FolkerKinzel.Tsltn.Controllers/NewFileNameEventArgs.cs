using System;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class NewFileNameEventArgs : EventArgs
    {
        internal NewFileNameEventArgs(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }
    }
}