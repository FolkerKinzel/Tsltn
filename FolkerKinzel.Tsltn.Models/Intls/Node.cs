using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class Node : INode
    {
        //private static readonly Dictionary<long, Node> _nodeContainer = new Dictionary<long, Node>();
        private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        private readonly string _nodePath;
        private readonly string _innerXml;
        private string? _innerText;

        private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this.XmlNode = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this.ID = Utility.GetNodeID(el, out _innerXml, out _nodePath);
            _innerXml = _whitespaceRegex.Replace(_innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");

            var firstNode = (Node?)Document.Instance.FirstNode;
            this.HasAncestor = !(firstNode is null || this.ID == firstNode.ID);

            this.HasDescendant = Document.GetNextNode(el) != null;
        }

        


        internal XElement XmlNode { get; }

        internal long ID { get; }

        public string NodePath => _nodePath;
        public string InnerXml => _innerXml;
        public string InnerText
        {
            get
            {
                this._innerText ??= _whitespaceRegex.Replace(XmlNode.Value, " ");
                return _innerText;
            }
        }


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


        public bool HasAncestor { get; }

        public bool HasDescendant { get; }



        public INode? GetAncestor()
        {
            if(!HasAncestor)
            {
                return null;
            }
            var el = Document.GetPreviousNode(XmlNode);
            return el is null ? null : new Node(el);
        }

        public INode? GetDescendant()
        {
            if(!HasDescendant)
            {
                return null;
            }
            var el = Document.GetNextNode(XmlNode);
            return el is null ? null : new Node(el);
        }



        public INode? GetNextUntranslated()
        {
            var el = Document.GetNextUntranslated(XmlNode);
            return el is null ? null : new Node(el);
        }


        public INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord)
        {
            XElement? node = Document.GetNextNode(this.XmlNode);

            while (node != null)
            {
                if (Utility.ContainsPathFragment(Utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
            }


            if (Utility.ContainsPathFragment(Document.Instance.FirstNode!.NodePath, nodePathFragment, ignoreCase, wholeWord))
            {
                return Document.Instance.FirstNode;
            }

            if(object.ReferenceEquals(this.XmlNode, ((Node?)Document.Instance.FirstNode)!.XmlNode))
            {
                return null;
            }
            

            node = Document.GetNextNode(((Node)Document.Instance.FirstNode).XmlNode);

            while (node != null && !object.ReferenceEquals(node, this.XmlNode))
            {
                if (Utility.ContainsPathFragment(Utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
            }

            return null;
        }

        
    }
}
