using FolkerKinzel.Tsltn.Models;
using System;

namespace Tsltn
{
    public class UntranslatedNodeEventArgs : EventArgs
    {
        public UntranslatedNodeEventArgs(INode? untranslatedNode)
        {
            this.UntranslatedNode = untranslatedNode;
        }

        public INode? UntranslatedNode { get; }
    }
}
