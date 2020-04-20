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
        private INode? _previousNode;
        private INode? _nextNode;
        private string? _innerText;

        private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this.XmlNode = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this.ID = Utility.GetNodeID(el, out _innerXml, out _nodePath);
            _innerXml = _whitespaceRegex.Replace(_innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");
        }


        //private Node(XElement el, long id, string innerXml, string nodePath)
        //{
        //    this.XmlNode = el;
        //    this.ID = id;
        //    this._innerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");
        //    this._nodePath = nodePath;
        //}


        internal XElement XmlNode { get; }

        //public Regex WhitespaceRegex => _whitespaceRegex;

        private long ID { get; }

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




        //private bool IsManualTranslation => Document.HasManualTranslation(_nodePathHash);


        public INode? PreviousNode
        {
            get
            {
                //_nodeContainer[ID] = this;

                if (PreviousNode != null)
                {
                    return PreviousNode;
                }

                var el = Document.GetPreviousNode(XmlNode);

                if (el is null)
                {
                    return null;
                }

                Node previousNode = new Node(el)
                {
                    NextNode = this
                };
                this.PreviousNode = previousNode;

                return previousNode;
            }
            private set => PreviousNode = value;
        }



        public INode? NextNode
        {
            get
            {
                //_nodeContainer[ID] = this;

                if (NextNode != null)
                {
                    return NextNode;
                }

                var el = Document.GetNextNode(XmlNode);

                if (el is null)
                {
                    return null;
                }

                Node? nextNode = new Node(el)
                {
                    PreviousNode = this
                };
                this.NextNode = nextNode;

                return nextNode;
            }
            private set => NextNode = value;
        }


        public INode? NextUntranslated
        {
            get
            {
                //_nodeContainer[ID] = this;

                var el = Document.GetNextUntranslated(XmlNode);

                return el is null ? null : new Node(el);
            }
        }


        public INode? FindNode(string nodePathFragment, bool ignoreCase)
        {
            //_nodeContainer[ID] = this;

            StringComparison comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            XElement? node = Document.GetNextNode(this.XmlNode);

            while (node != null)
            {
                string path = Utility.GetNodePath(node);

                if (path.Contains(nodePathFragment, comp))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
            }


            if (Document.Instance.FirstNode!.NodePath.Contains(nodePathFragment, comp))
            {
                return Document.Instance.FirstNode;
            }

            if (object.ReferenceEquals(this, Document.Instance.FirstNode))
            {
                return null;
            }

            node = Document.GetNextNode(((Node)Document.Instance.FirstNode).XmlNode);

            while (node != null && !object.ReferenceEquals(node, this))
            {
                string path = Utility.GetNodePath(node);

                if (path.Contains(nodePathFragment, comp))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
            }

            return null;
        }


        //internal static void ClearNodeContainer()
        //{
        //    _nodeContainer.Clear();
        //    _nodeContainer.TrimExcess();
        //}


        //[return: NotNullIfNotNull("el")]
        //internal static Node? GetNode(XElement? el)
        //{
        //    if (el is null)
        //    {
        //        return null;
        //    }

        //    long nodeID = Utility.GetNodeID(el, out string innerXml, out string nodePath);

        //    return _nodeContainer.ContainsKey(nodeID) ? _nodeContainer[nodeID] : new Node(el, nodeID, innerXml, nodePath);
        //}


        


    }
}
