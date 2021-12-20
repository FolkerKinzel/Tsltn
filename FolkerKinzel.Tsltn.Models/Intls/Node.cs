using FolkerKinzel.XmlFragments;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class Node : INode
    {
        private readonly string _nodePath;
        private readonly string _innerXml;
        private readonly IDocumentNodes _doc;


        //private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el, IDocumentNodes doc)
        {
            _doc = doc;
            XmlNode = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            ID = _doc.GetNodeID(el, out _innerXml, out _nodePath);

            //_innerXml = _innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal).Trim();
            _innerXml = XmlFragmentBeautifier.Beautify(_innerXml);

            HasAncestor = !(_doc.FirstNode is null) && !ReferencesSameXml(_doc.FirstNode);

            HasDescendant = _doc.GetNextNode(el) != null;
        }


        internal XElement XmlNode { get; }

        private long ID { get; }

        public string NodePath => _nodePath;
        public string InnerXml => _innerXml;



        public string? Translation
        {
            get => _doc.TryGetTranslation(ID, out string? transl) ? transl : null;

            set
            {
                if (!StringComparer.Ordinal.Equals(value, Translation))
                {
                    _doc.SetTranslation(ID, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReferencesSameXml(INode? other) => object.ReferenceEquals(XmlNode, (other as Node)?.XmlNode);

        public bool HasAncestor { get; }

        public bool HasDescendant { get; }



        public INode? GetAncestor()
        {
            if (!HasAncestor)
            {
                return null;
            }
            XElement? el = _doc.GetPreviousNode(XmlNode);
            return el is null ? null : new Node(el, _doc);
        }

        public INode? GetDescendant()
        {
            if (!HasDescendant)
            {
                return null;
            }
            XElement? el = _doc.GetNextNode(XmlNode);
            return el is null ? null : new Node(el, _doc);
        }



        public INode? GetNextUntranslated()
        {
            XElement? unTrans = _doc.GetNextNode(XmlNode);
            while (unTrans != null)
            {
                if (!_doc.GetHasTranslation(_doc.GetNodeID(unTrans)))
                {
                    return new Node(unTrans, _doc);
                }

                unTrans = _doc.GetNextNode(unTrans);
            }

            var firstNode = (Node?)_doc.FirstNode;

            if (firstNode != null)
            {
                if (!_doc.GetHasTranslation(firstNode.ID))
                {
                    return firstNode;
                }
            }
            else
            {
                return null; // Kann nur sein, wenn ein anderer Thread derweil Document.CloseTsltn aufgerufen hat.
            }


            unTrans = _doc.GetNextNode(firstNode.XmlNode);


            while (unTrans != null && !object.ReferenceEquals(unTrans, this.XmlNode))
            {
                if (!_doc.GetHasTranslation(_doc.GetNodeID(unTrans)))
                {
                    return new Node(unTrans, _doc);
                }

                unTrans = _doc.GetNextNode(unTrans);
            }

            return !_doc.GetHasTranslation(ID) ? this : null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord)
            => _doc.FindNode(XmlNode, nodePathFragment, ignoreCase, wholeWord);

    }
}
