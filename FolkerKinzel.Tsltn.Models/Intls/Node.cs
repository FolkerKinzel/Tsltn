﻿using FolkerKinzel.XmlFragments;
using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal class Node : INode
    {
        private readonly string _nodePath;
        private readonly string _innerXml;
        private readonly ITranslation _transl;
        private readonly XmlNavigator _nav;
        private readonly Node _firstNode;
        private readonly XElement _xElement;


        internal Node(XElement el, ITranslation transl, XmlNavigator navigator, Node firstNode)
        {
            _nav = navigator;
            _firstNode = firstNode;
            _xElement = el;
            _transl = transl;

            // Das Replacement des geschützten Leerzeichens soll beim Hashen
            // ignoriert werden:
            ID = _nav.GetNodeID(el, out _innerXml, out _nodePath);

            //_innerXml = _innerXml.Replace("\u00A0", NonBreakingSpace, StringComparison.Ordinal).Trim();
            _innerXml = XmlFragmentBeautifier.Beautify(_innerXml);

            HasAncestor = !ReferencesSameXml(_firstNode);

            HasDescendant = _nav.GetNextXElement(el) != null;
        }



        private long ID { get; }

        public string NodePath => _nodePath;

        public string InnerXml => _innerXml;



        public string? Translation
        {
            get => _transl.TryGetTranslation(ID, out string? transl) ? transl : null;

            set
            {
                if (!StringComparer.Ordinal.Equals(value, Translation))
                {
                    _transl.SetTranslation(ID, value);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReferencesSameXml(INode? other) => object.ReferenceEquals(_xElement, (other as Node)?._xElement);

        public bool HasAncestor { get; }

        public bool HasDescendant { get; }



        public INode? GetAncestor()
        {
            if (!HasAncestor)
            {
                return null;
            }
            XElement? el = _nav.GetPreviousXElement(_xElement);
            return el is null ? null : new Node(el, _transl, _nav, _firstNode);
        }

        public INode? GetDescendant()
        {
            if (!HasDescendant)
            {
                return null;
            }
            XElement? el = _nav.GetNextXElement(_xElement);
            return el is null ? null : new Node(el, _transl, _nav, _firstNode);
        }



        public INode? GetNextUntranslated()
        {
            XElement? unTrans = _nav.GetNextXElement(_xElement);
            while (unTrans != null)
            {
                if (!_transl.GetHasTranslation(_nav.GetNodeID(unTrans)))
                {
                    return new Node(unTrans, _transl, _nav, _firstNode);
                }

                unTrans = _nav.GetNextXElement(unTrans);
            }


            if (!_transl.GetHasTranslation(_firstNode.ID))
            {
                return _firstNode;
            }


            unTrans = _nav.GetNextXElement(_firstNode._xElement);


            while (unTrans != null && !object.ReferenceEquals(unTrans, this._xElement))
            {
                if (!_transl.GetHasTranslation(_nav.GetNodeID(unTrans)))
                {
                    return new Node(unTrans, _transl, _nav, _firstNode);
                }

                unTrans = _nav.GetNextXElement(unTrans);
            }

            return !_transl.GetHasTranslation(ID) ? this : null;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord)
        {
            if (Utility.ContainsPathFragment(_firstNode.NodePath, nodePathFragment, ignoreCase, wholeWord))
            {
                return _firstNode;
            }

            XElement? node = _nav.GetNextXElement(_firstNode._xElement);

            while (node != null)
            {
                if (Utility.ContainsPathFragment(_nav.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node, _transl, _nav, _firstNode);
                }

                node = _nav.GetNextXElement(node);
            }

            return null;
        }


    }
}
