using FolkerKinzel.Tsltn.Models;
using System;

namespace Tsltn
{
    public class NavigationRequestedEventArgs : EventArgs
    {
        public NavigationRequestedEventArgs(string pathFragment, bool caseSensitive)
        {
            this.PathFragment = pathFragment;
            this.CaseSensitive = caseSensitive;
        }

        public string PathFragment { get; }
        public bool CaseSensitive { get; }
    }
}