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
        private static readonly Dictionary<long, Node> _nodeContainer = new Dictionary<long, Node>();
        private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        private readonly int _nodePathHash;
        private readonly int _contentHash;

        private INode? _previousNode;
        private INode? _nextNode;
        private string? _innerText;

        private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this.XmlNode = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this._contentHash = Utility.GetContentHash(el, out string innerXml);
            this.InnerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");


            this.NodePath = Utility.GetNodePath(el);
            this._nodePathHash = Utility.GetNodePathHash(NodePath);
        }


        private Node(XElement el, string innerXml, int nodePathHash, int contentHash)
        {
            this.XmlNode = el;

            this.InnerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");
            this.NodePath = Utility.GetNodePath(el);
            this._contentHash = contentHash;
            this._nodePathHash = nodePathHash;
        }


        internal XElement XmlNode { get; }

        //public Regex WhitespaceRegex => _whitespaceRegex;

        private long ID => CreateNodeID(_nodePathHash, _contentHash);

        public string NodePath { get; }

        public string InnerXml { get; }

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
            get =>
            Document.TryGetManualTranslation(_nodePathHash, out string? manualTransl)
            ? manualTransl
            : Document.GetAutoTranslation(_contentHash);

            set
            {
                value ??= "";

                if (!value.Equals(Translation, StringComparison.Ordinal))
                {
                    if (Document.HasAutoTranslation(_contentHash))
                    {
                        Document.SetManualTranslation(_nodePathHash, value);
                    }
                    else
                    {
                        Document.SetManualTranslation(_nodePathHash, null);
                        Document.SetAutoTranslation(_contentHash, value);
                    }
                }
            }
        }


        

        //private bool IsManualTranslation => Document.HasManualTranslation(_nodePathHash);


        public INode? PreviousNode
        {
            get
            {
                _nodeContainer[ID] = this;

                if (_previousNode != null)
                {
                    return _previousNode;
                }

                var el = Document.GetPreviousNode(XmlNode);

                if (el is null)
                {
                    return null;
                }

                Node previousNode = GetNode(el);

                previousNode.NextNode = this;
                this.PreviousNode = previousNode;

                return previousNode;
            }
            private set => _previousNode = value;
        }



        public INode? NextNode
        {
            get
            {
                _nodeContainer[ID] = this;

                if (_nextNode != null)
                {
                    return _nextNode;
                }

                var el = Document.GetNextNode(XmlNode);

                if (el is null)
                {
                    return null;
                }

                Node? nextNode = GetNode(el);

                nextNode.PreviousNode = this;
                this.NextNode = nextNode;

                return nextNode;
            }
            private set => _nextNode = value;
        }


        public INode? NextUntranslated
        {
            get
            {
                _nodeContainer[ID] = this;

                var el = Document.GetNextUntranslated(XmlNode);

                return GetNode(el);
            }
        }


        public INode? FindNode(string nodePathFragment, bool ignoreCase)
        {
            _nodeContainer[ID] = this;

            StringComparison comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            XElement? node = Document.GetNextNode(this.XmlNode);

            while (node != null)
            {
                string path = Utility.GetNodePath(node);

                if (path.Contains(nodePathFragment, comp))
                {
                    return GetNode(node);
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
                    return GetNode(node);
                }

                node = Document.GetNextNode(node);
            }

            return null;
        }


        internal static void ClearNodeContainer()
        {
            _nodeContainer.Clear();
            _nodeContainer.TrimExcess();
        }


        [return: NotNullIfNotNull("el")]
        internal static Node? GetNode(XElement? el)
        {
            if (el is null)
            {
                return null;
            }

            int nodePathHash = Utility.GetNodePathHash(el);
            int contentHash = Utility.GetContentHash(el, out string innerXml);
            long nodeID = CreateNodeID(nodePathHash, contentHash);

            return _nodeContainer.ContainsKey(nodeID) ? _nodeContainer[nodeID] : new Node(el, innerXml, nodePathHash, contentHash);
        }


        private static long CreateNodeID(int nodePathHash, int contentHash)
        {
            long id = (uint)nodePathHash;

            id <<= 32;

            id |= (uint)contentHash;

            return id;
        }


    }
}
