using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class XCodeCloneElement : XElement
    {
        private static readonly XElement _emptyCodeBlock = new XElement(Sandcastle.CODE);


        internal XCodeCloneElement(XElement source) : base(source)
        {
            this.Source = source;

            foreach (var codeNode in Elements(Sandcastle.CODE).ToArray())
            {
                codeNode.ReplaceWith(_emptyCodeBlock);
            }
        }

        internal XElement Source { get; }
    }
}
