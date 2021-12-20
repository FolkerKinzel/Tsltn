using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal interface IDocumentNodes
    {
        long GetNodeID(XElement node, out string innerXml, out string nodePath);

        long GetNodeID(XElement node);


        XElement? GetNextNode(XElement? currentNode);

        INode? FirstNode { get; }

        bool GetHasTranslation(long id);


        bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl);

        void SetTranslation(long nodeID, string? transl);

        XElement? GetPreviousNode(XElement? currentNode);

        INode? FindNode(XElement current, string nodePathFragment, bool ignoreCase, bool wholeWord);






    }
}
