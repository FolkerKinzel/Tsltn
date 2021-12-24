using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using FolkerKinzel.Strings;

namespace FolkerKinzel.Tsltn.Models.Intls;

/// <summary>
/// Provides synchronized access to the XML DOM.
/// </summary>
public class XmlNavigator : ICloneable
{
    private const int LIST_MAX_CAPACITY = 8;
    private const int STRING_BUILDER_MAX_CAPACITY = 1024;

    private readonly object _lockObject = new();
    private readonly XDocument _xmlDocument;
    private readonly StringBuilder _sb = new(STRING_BUILDER_MAX_CAPACITY);
    private readonly List<string> _list = new(LIST_MAX_CAPACITY);

    private XmlNavigator(XDocument xmlDocument)
    {
        _xmlDocument = xmlDocument;
    }

    public static XmlNavigator? Load(string? xmlFilePath)
    {
        if (!File.Exists(xmlFilePath))
        {
            return null;
        }

        var xDocument = XDocument.Load(xmlFilePath, LoadOptions.None);

        return new XmlNavigator(xDocument);
    }

    public object Clone()
    {
        lock (_lockObject)
        {
            return new XmlNavigator(new XDocument(_xmlDocument));
        }
    }

    public void SaveXml(string filePath)
    {
        lock (_lockObject)
        {
            _xmlDocument.Save(filePath);
        }
    }

    public XElement? GetFirstXElement()
    {
        lock (_lockObject)
        {
            IEnumerable<XElement>? members = _xmlDocument.Root.Element(Sandcastle.MEMBERS)?.Elements(Sandcastle.MEMBER);

            if (members is null)
            {
                return null;
            }

            foreach (XElement member in members)
            {
                foreach (XElement section in member.Elements())
                {
                    XElement? el = ExtractDescendant(section);

                    if (el != null)
                    {
                        return el;
                    }
                }
            }
        }
        return null;
    }

    public XElement? GetNextXElement(XElement? current)
    {
        while (true)
        {
            if (current is XCodeCloneElement clone)
            {
                current = clone.Source;
            }

            if (current is null || current.Name == Sandcastle.MEMBERS)
            {
                break;
            }

            lock (_lockObject)
            {
                foreach (XElement sibling in current.ElementsAfterSelf())
                {
                    XElement? el = ExtractDescendant(sibling);

                    if (el != null)
                    {
                        return el;
                    }
                }
            }

            current = current.Parent;
        }
        return null;
    }

    public XElement? GetPreviousXElement(XElement? current)
    {
        while (true)
        {
            if (current is XCodeCloneElement clone)
            {
                current = clone.Source;
            }

            if (current is null || current.Name == Sandcastle.MEMBERS)
            {
                break;
            }

            lock (_lockObject)
            {
                foreach (XElement? sibling in current.ElementsBeforeSelf().Reverse())
                {
                    XElement? el = ExtractAncestor(sibling);

                    if (el != null)
                    {
                        return el;
                    }
                }
            }

            current = current.Parent;
        }
        return null;
    }

    public long GetNodeID(XElement node, out string innerXml, out string nodePath)
    {
        nodePath = GetNodePath(node);
        int nodePathHash = GetNodePathHash(nodePath);
        int contentHash = GetContentHash(node, out innerXml);
        return XmlUtility.ComputeNodeID(nodePathHash, contentHash);
    }

    public long GetNodeID(XElement node)
    {
        int nodePathHash = GetNodePathHash(node);
        int contentHash = GetContentHash(node, out _);
        return XmlUtility.ComputeNodeID(nodePathHash, contentHash);
    }

    public string GetNodePath(XElement? node)
    {
        FillStringBuilder(node);
        return _sb.ToString();
    }

    #region private

    private int GetNodePathHash(XElement? node)
    {
        FillStringBuilder(node);
        return _sb.GetPersistentHashCode(HashType.Ordinal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetNodePathHash(string nodePath) => nodePath.GetPersistentHashCode(HashType.Ordinal);

    private static int GetContentHash(XElement node, out string innerXml)
    {
        innerXml = node.InnerXml();
        return innerXml.GetPersistentHashCode(HashType.AlphaNumericIgnoreCase);
    }

    private void FillStringBuilder(XElement? node)
    {
        lock (_lockObject)
        {
            _ = _sb.Clear();
            _list.Clear();

            if (node is null)
            {
                return;
            }

            if (node is XCodeCloneElement clone)
            {
                node = clone.Source;
            }


            while (true)
            {
                if (node is null || node.Name.LocalName == Sandcastle.MEMBERS)
                {
                    break;
                }

                string name = node.Name.LocalName;

                switch (name)
                {
                    case Sandcastle.MEMBER:
                        _list.Add(node.Attribute(Sandcastle.NameAttribute)?.Value ?? name);
                        break;
                    case Sandcastle.EVENT:
                    case Sandcastle.EXCEPTION:
                    case Sandcastle.PERMISSION:
                    case Sandcastle.SEEALSO:
                        //case Sandcastle.SEE:
                        _list.Add($"{name}={node.Attribute(Sandcastle.CrefAttribute)?.Value}");
                        break;
                    case Sandcastle.PARAM:
                    case Sandcastle.TYPEPARAM:
                        //case Sandcastle.PARAMREF:
                        //case Sandcastle.TYPE_PARAMREF:
                        _list.Add($"{name}={node.Attribute(Sandcastle.NameAttribute)?.Value}");
                        break;
                    case Sandcastle.REVISION:
                        _list.Add($"{name}={node.Attribute(Sandcastle.VersionAttribute)?.Value}");
                        break;
                    case Sandcastle.CONCEPTUAL_LINK:
                        _list.Add($"{name}={node.Attribute(Sandcastle.TargetAttribute)?.Value}");
                        break;
                    default:
                        _list.Add(name);
                        break;
                }

                node = node.Parent;
            }

            int last = _list.Count - 1;
            for (int i = last; i >= 0; i--)
            {
                if (i == last)
                {
                    string memberName = _list[last];
                    int length = memberName.Length;

                    if (length > 2)
                    {
                        // Membernamen fangen mit der Art des Members an, z.B. "T:" für Type oder "P:" für Property.
                        // Das ist für die Übersetzung nicht relevant und kann weggelassen werden.

                        _ = _sb.Append(memberName, 2, length - 2);
                    }
                    else
                    {
                        _ = _sb.Append(memberName);
                    }

                }
                else
                {
                    _ = _sb.Append(_list[i]);
                }

                if (i != 0)
                {
                    _ = _sb.Append('/');
                }
            }
        }
    }

    #region private static

    private static XElement? ExtractDescendant(XElement section)
    {
        // Die Reihenfolge ist entscheidend, denn 
        // Utility.IsTranslatable führt nur einen 
        // knappen Negativtest durch!
        string name = section.Name.LocalName;
        if (XmlUtility.IsContainerSection(name))
        {
            return MaskCodeBlock(ExtractDescendantFromContainer(section));
        }
        else if (XmlUtility.IsTranslatable(name))
        {
            return MaskCodeBlock(section);
        }

        return null;

        /////////////////////////////////////////////////////////////

        static XElement? ExtractDescendantFromContainer(XElement container)
        {
            foreach (XElement innerElement in container.Elements())
            {
                string innerElementName = innerElement.Name.LocalName;
                if (XmlUtility.IsContainerSection(innerElementName))
                {
                    return ExtractDescendantFromContainer(innerElement);
                }
                else if (XmlUtility.IsTranslatable(innerElementName))
                {
                    return innerElement;
                }
            }

            return null;
        }
    }

    private static XElement? ExtractAncestor(XElement section)
    {
        // Die Reihenfolge ist entscheidend, denn 
        // Utility.IsTranslatable führt nur einen 
        // knappen Negativtest durch!
        if (XmlUtility.IsContainerSection(section.Name.LocalName))
        {
            return MaskCodeBlock(ExtractAncestorFromContainer(section));
        }
        else if (XmlUtility.IsTranslatable(section.Name.LocalName))
        {
            return MaskCodeBlock(section);
        }

        return null;

        ///////////////////////////////////////////////////////////////

        static XElement? ExtractAncestorFromContainer(XElement container)
        {
            foreach (XElement innerElement in container.Elements().Reverse())
            {
                if (XmlUtility.IsContainerSection(innerElement.Name.LocalName))
                {
                    return ExtractAncestorFromContainer(innerElement);
                }
                else if (XmlUtility.IsTranslatable(innerElement.Name.LocalName))
                {
                    return innerElement;
                }
            }

            return null;
        }
    }

    [return: NotNullIfNotNull("node")]
    private static XElement? MaskCodeBlock(XElement? node)
    {
        if (node is null)
        {
            return null;
        }

        return node.Element(Sandcastle.CODE) != null ? new XCodeCloneElement(node) : node;
    }

    #endregion
    #endregion
}
