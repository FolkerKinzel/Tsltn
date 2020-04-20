using FolkerKinzel.Tsltn.Models;
using System;

namespace Tsltn
{
    public class NavigationRequestedEventArgs : EventArgs
    {
        public NavigationRequestedEventArgs(string pathFragment, bool caseSensitive, bool wholeWord)
        {
            this.PathFragment = pathFragment;
            this.CaseSensitive = caseSensitive;
            this.WholeWord = wholeWord;
        }

        public string PathFragment { get; }

        public bool CaseSensitive { get; }

        public bool WholeWord { get; }
    }
}