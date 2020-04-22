using System;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class BadFileNameEventArgs : EventArgs
    {
        internal BadFileNameEventArgs(string fileName)
        {
            this.FileName = fileName;
        }

        public string FileName { get; }
    }
}