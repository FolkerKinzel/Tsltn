using System;

namespace FolkerKinzel.Tsltn.Controllers
{
    public class HasContentChangedEventArgs : EventArgs
    {
        public HasContentChangedEventArgs(bool hasContent)
        {
            this.HasContent = hasContent;
        }

        public bool HasContent { get; }
    }
}