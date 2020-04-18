using FolkerKinzel.Tsltn.Models;
using System;

namespace Tsltn
{
    public class NavigationRequestedEventArgs : EventArgs
    {
        public NavigationRequestedEventArgs(INode? target, string pathFragment)
        {
            this.Target = target;
            this.PathFragment = pathFragment;
        }

        public INode? Target { get; }
        public string PathFragment { get; }
    }
}