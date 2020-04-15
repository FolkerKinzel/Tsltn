using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
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


        private string? _manualTranslation;


        private const string NonBreakingSpace = "&#160;";


        internal Node(XElement el)
        {
            this._xElement = el;

            string innerXml = el.InnerXml();
            this.InnerXml = _whitespaceRegex.Replace(innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal), " ");
            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            this._contentHash = Utility.GetContentHash(innerXml);

            this.NodePath = Utility.GetNodePath(el);
            this._nodePathHash = Utility.GetNodePathHash(NodePath);

            //this.ID = CreateNodeID(_nodePathHash, _contentHash);

            this.IsManualTranslation = Document.TryGetManualTranslation(_nodePathHash, out _manualTranslation);

        }

        public long ID => CreateNodeID(_nodePathHash, _contentHash);

        public string NodePath { get; }

        public string InnerXml { get; }

        public string InnerText => _xElement.Value;


        public string? ManualTranslation
        {
            get { return _manualTranslation; }
            set { _manualTranslation = value; }
        }


        public string? AutoTranslation
        {
            get => Document.GetAutoTranslation(_contentHash);
            set => Document.SetAutoTranslation(_contentHash, value);
        }

        public bool IsManualTranslation { get; set; }


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

                var previousNode = new Node(el);

                previousNode = _nodeContainer.ContainsKey(previousNode.ID) ? _nodeContainer[previousNode.ID] : previousNode;

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

                var nextNode = new Node(el);

                nextNode = _nodeContainer.ContainsKey(nextNode.ID) ? _nodeContainer[nextNode.ID] : nextNode;

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

                var untranslated = new Node(el);

                untranslated = _nodeContainer.ContainsKey(untranslated.ID) ? _nodeContainer[untranslated.ID] : untranslated;

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
