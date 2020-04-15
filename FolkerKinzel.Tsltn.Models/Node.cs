using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    public class Node
    {
        private static readonly Dictionary<long, Node> _nodeContainer = new Dictionary<long, Node>();

        private static readonly Regex _whitespaceRegex = new Regex(@"\s+", RegexOptions.Compiled);

        private readonly XElement _xElement;
        private readonly int _nodePathHash;
        private readonly int _contentHash;

        private Node? _previousNode;
        private Node? _nextNode;


        private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this._xElement = el;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this._contentHash = Utility.GetContentHash(el, out string innerXml);
            this.InnerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");


            this.NodePath = Utility.GetNodePath(el);
            this._nodePathHash = Utility.GetNodePathHash(NodePath);
        }


        private Node(XElement el, string innerXml, int nodePathHash, int contentHash)
        {
            this._xElement = el;

            this.InnerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");
            this.NodePath = Utility.GetNodePath(el);
            this._contentHash = contentHash;
            this._nodePathHash = nodePathHash;
        }

        public long ID => CreateNodeID(_nodePathHash, _contentHash);

        public string NodePath { get; }

        public string InnerXml { get; }

        public string InnerText => _xElement.Value;


        [DisallowNull]
        public string? Translation
        {
            get => Document.TryGetManualTranslation(_nodePathHash, out string? manualTransl) ? manualTransl : Document.GetAutoTranslation(_contentHash);

            set
            {
                Debug.Assert(value != null);

                if(Document.HasAutoTranslation(_contentHash))
                {
                    Document.SetManualTranslation(_nodePathHash, value);
                }
                else
                {
                    Document.SetAutoTranslation(_contentHash, value);
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetToAutoTranslation() => Document.SetManualTranslation(_nodePathHash, null);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResetTranslation()
        {
            Document.SetManualTranslation(_nodePathHash, null);
            Document.SetAutoTranslation(_contentHash, null);
        }
        

        public bool IsManualTranslation => Document.HasManualTranslation(_nodePathHash);


        public Node? PreviousNode
        {
            get
            {
                _nodeContainer[ID] = this;

                if (_previousNode != null)
                {
                    return _previousNode;
                }

                var el = Document.GetPreviousNode(_xElement);

                if (el is null)
                {
                    return null;
                }


                int nodePathHash = Utility.GetNodePathHash(el);
                int contentHash = Utility.GetContentHash(el, out string innerXml);
                long nodeID = CreateNodeID(nodePathHash, contentHash);

                Node previousNode = _nodeContainer.ContainsKey(nodeID) ? _nodeContainer[nodeID] : new Node(el, innerXml, nodePathHash, contentHash);

                previousNode.NextNode = this;
                this.PreviousNode = previousNode;

                return previousNode;
            }
            private set => _previousNode = value;
        }



        public Node? NextNode
        {
            get
            {
                _nodeContainer[ID] = this;

                if (_nextNode != null)
                {
                    return _nextNode;
                }

                var el = Document.GetNextNode(_xElement);

                if (el is null)
                {
                    return null;
                }

                int nodePathHash = Utility.GetNodePathHash(el);
                int contentHash = Utility.GetContentHash(el, out string innerXml);
                long nodeID = CreateNodeID(nodePathHash, contentHash);

                Node nextNode = _nodeContainer.ContainsKey(nodeID) ? _nodeContainer[nodeID] : new Node(el, innerXml, nodePathHash, contentHash);

                nextNode.PreviousNode = this;
                this.NextNode = nextNode;

                return nextNode;
            }
            private set => _nextNode = value;
        }


        public Node? NextUntranslated
        {
            get
            {
                _nodeContainer[ID] = this;

                var el = Document.GetNextUntranslated(_xElement);

                if (el is null)
                {
                    return null;
                }

                int nodePathHash = Utility.GetNodePathHash(el);
                int contentHash = Utility.GetContentHash(el, out string innerXml);
                long nodeID = CreateNodeID(nodePathHash, contentHash);

                Node untranslated = _nodeContainer.ContainsKey(nodeID) ? _nodeContainer[nodeID] : new Node(el, innerXml, nodePathHash, contentHash);

                return untranslated;
            }
        }


        internal static void ClearNodeContainer()
        {
            _nodeContainer.Clear();
            _nodeContainer.TrimExcess();
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
