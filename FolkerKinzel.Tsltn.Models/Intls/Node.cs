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

        //private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this.XmlNode = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this.ID = Document.GetNodeID(el, out _innerXml, out _nodePath);

            //_innerXml = _innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal).Trim();
            _innerXml = XmlFragmentBeautifier.Beautify(_innerXml);

            this.HasAncestor = !(Document.Instance.FirstNode is null) && !ReferencesSameXml(Document.Instance.FirstNode);

            this.HasDescendant = Document.GetNextNode(el) != null;
        }


        internal XElement XmlNode { get; }

        private long ID { get; }

        public string NodePath => _nodePath;
        public string InnerXml => _innerXml;



        public string? Translation
        {
            get => Document.TryGetTranslation(ID, out string? transl) ? transl : null;

            set
            {
                if (!StringComparer.Ordinal.Equals(value, Translation))
                {
                    Document.SetTranslation(ID, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReferencesSameXml(INode? other) => object.ReferenceEquals(this.XmlNode, (other as Node)?.XmlNode);

        public bool HasAncestor { get; }

        public bool HasDescendant { get; }



        public INode? GetAncestor()
        {
            if (!HasAncestor)
            {
                return null;
            }
            XElement? el = Document.GetPreviousNode(XmlNode);
            return el is null ? null : new Node(el);
        }

        public INode? GetDescendant()
        {
            if (!HasDescendant)
            {
                return null;
            }
            XElement? el = Document.GetNextNode(XmlNode);
            return el is null ? null : new Node(el);
        }



        public INode? GetNextUntranslated()
        {
            XElement? unTrans = Document.GetNextNode(this.XmlNode);
            while (unTrans != null)
            {
                if (!Document.GetHasTranslation(Document.GetNodeID(unTrans)))
                {
                    return new Node(unTrans);
                }

                unTrans = Document.GetNextNode(unTrans);
            }

            var firstNode = (Node?)Document.Instance.FirstNode;

            if (firstNode != null)
            {
                if (!Document.GetHasTranslation((firstNode.ID)))
                {
                    return firstNode;
                }
            }
            else
            {
                return null; // Kann nur sein, wenn ein anderer Thread derweil Document.CloseTsltn aufgerufen hat.
            }


            unTrans = Document.GetNextNode(firstNode.XmlNode);


            while (unTrans != null && !object.ReferenceEquals(unTrans, this.XmlNode))
            {
                if (!Document.GetHasTranslation(Document.GetNodeID(unTrans)))
                {
                    return new Node(unTrans);
                }

                unTrans = Document.GetNextNode(unTrans);
            }

            if (!Document.GetHasTranslation(this.ID))
            {
                return this;
            }

            return null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord) => Document.FindNode(this.XmlNode, nodePathFragment, ignoreCase, wholeWord);

    }
}
