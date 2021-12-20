using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal interface ITranslation
    {
        //long GetNodeID(XElement node, out string innerXml, out string nodePath);

        //long GetNodeID(XElement node);


        //XElement? GetNextXElement(XElement? current);

        //XElement? GetPreviousXElement(XElement? current);

        //INode? FindNode(XElement current, string nodePathFragment, bool ignoreCase, bool wholeWord);

        //XmlNavigator? Navigator { get; }

        //INode? FirstNode { get; }

        bool GetHasTranslation(long id);

        void SetTranslation(long nodeID, string? transl);


        bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl);









    }
}
