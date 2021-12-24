using System.Xml.Linq;
using FolkerKinzel.XmlFragments;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal class Node : INode
{
    private readonly string _nodePath;
    private readonly string _innerXml;
    private readonly ITranslation _transl;
    private readonly XmlNavigator _nav;
    private readonly Node _firstNode;
    private readonly XElement _xElement;
    private readonly long _id;

    internal Node(XElement el, ITranslation transl, XmlNavigator navigator, Node? firstNode)
    {
        _nav = navigator;
        _firstNode = firstNode ?? this;
        _xElement = el;
        _transl = transl;

        // Das Replacement des geschützten Leerzeichens soll beim Hashen
        // ignoriert werden:
        _id = _nav.GetNodeID(el, out _innerXml, out _nodePath);

        _innerXml = XmlFragmentBeautifier.Beautify(_innerXml);

        HasAncestor = !Equals(_firstNode);

        HasDescendant = _nav.GetNextXElement(el) != null;
    }

    public string NodePath => _nodePath;

    public string InnerXml => _innerXml;

    public string? Translation
    {
        get => _transl.TryGetTranslation(_id, out string? transl) ? transl : null;

        set
        {
            if (!StringComparer.Ordinal.Equals(value, Translation))
            {
                _transl.SetTranslation(_id, value);
            }
        }
    }

    public bool HasAncestor { get; }

    public bool HasDescendant { get; }

    public override bool Equals(object obj) => obj is Node node && Equals(node);

    public bool Equals(Node? node)
        => node is not null && object.ReferenceEquals(_xElement, node._xElement);

    public override int GetHashCode() => _xElement.GetHashCode();

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

        if (!_transl.GetHasTranslation(_firstNode._id))
        {
            return _firstNode;
        }

        unTrans = _nav.GetNextXElement(_firstNode._xElement);

        while (unTrans != null && !object.ReferenceEquals(unTrans, _xElement))
        {
            if (!_transl.GetHasTranslation(_nav.GetNodeID(unTrans)))
            {
                return new Node(unTrans, _transl, _nav, _firstNode);
            }

            unTrans = _nav.GetNextXElement(unTrans);
        }

        return !_transl.GetHasTranslation(_id) ? this : null;
    }


    public INode? FindNode(string nodePathFragment, bool ignoreCase, bool wholeWord)
    {
        XElement? node = _nav.GetNextXElement(_xElement);

        while (node != null)
        {
            if (XmlUtility.ContainsPathFragment(_nav.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
            {
                return new Node(node, _transl, _nav, _firstNode);
            }

            node = _nav.GetNextXElement(node);
        }

        if (XmlUtility.ContainsPathFragment(_firstNode.NodePath, nodePathFragment, ignoreCase, wholeWord))
        {
            return _firstNode;
        }

        if (Equals(_firstNode))
        {
            return null;
        }

        node = _nav.GetNextXElement(_firstNode._xElement);

        while (node != null && !object.ReferenceEquals(node, _xElement))
        {
            if (XmlUtility.ContainsPathFragment(_nav.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
            {
                return new Node(node, _transl, _nav, _firstNode);
            }

            node = _nav.GetNextXElement(node);
        }

        return null;
    }

}
