using FolkerKinzel.Strings;
using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace FolkerKinzel.Tsltn.Models
{
    public partial class Document : IDocument, IFileAccess
    {
        private static class Utility
        {
            private const int LIST_MAX_CAPACITY = 8;
            private const int STRING_BUILDER_MAX_CAPACITY = 1024;

            private static readonly StringBuilder _sb = new StringBuilder(STRING_BUILDER_MAX_CAPACITY);
            private static readonly List<string> _list = new List<string>(LIST_MAX_CAPACITY);

            private static readonly object _lockObject = new object();

            internal static void Cleanup()
            {
                lock (_lockObject)
                {
                    if (_sb.Capacity > STRING_BUILDER_MAX_CAPACITY)
                    {
                        Debug.WriteLine("Reset StringBuilder Capacity", "Utility");
                        _sb.Clear();
                        _sb.Capacity = STRING_BUILDER_MAX_CAPACITY;
                    }

                    if (_list.Capacity > LIST_MAX_CAPACITY)
                    {
                        Debug.WriteLine("Reset List Capacity", "Utility");
                        _list.Clear();
                        _list.Capacity = LIST_MAX_CAPACITY;
                    }
                }
            }


            internal static bool ContainsPathFragment(string nodePath, string pathFragment, bool ignoreCase, bool wholeWord)
            {
                if (wholeWord)
                {
                    string regex;
                    lock (_lockObject)
                    {
                        _sb.Clear();
                        _sb.Append(@"\b").Append(Regex.Escape(pathFragment)).Append(@"\b");

                        regex = _sb.ToString();
                    }

                    RegexOptions options = RegexOptions.Singleline | RegexOptions.CultureInvariant;

                    if (ignoreCase)
                    {
                        options |= RegexOptions.IgnoreCase;
                    }

                    return Regex.IsMatch(nodePath, regex, options);
                }
                else
                {
                    StringComparison comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    return nodePath.Contains(pathFragment, comp);
                }
            }

            internal static bool IsValidXml(string translation, [NotNullWhen(false)] out string? exceptionMessage)
            {
                exceptionMessage = null;

                try
                {
                    lock (_lockObject)
                    {
                        _sb.Clear().Append("<R>").Append(translation).Append("</R>");
                        _ = XElement.Parse(_sb.ToString(), LoadOptions.None);
                    }
                }
                catch (XmlException e)
                {
                    exceptionMessage = e.Message;
                    return false;
                }
                catch (Exception)
                {

                }
                return true;
            }


            /// <summary>
            /// Ersetzt das innere Xml von <paramref name="node"/> durch das in 
            /// <paramref name="translation"/> enthaltene Xml.
            /// </summary>
            /// <param name="node">Das <see cref="XElement"/>, das überstzt wird.</param>
            /// <param name="translation">XML, das den übersetzten Inhalt von <paramref name="node"/>
            /// bilden soll.</param>
            /// <exception cref="XmlException">Der Inhalt von <paramref name="translation"/> war kein gültiges Xml.</exception>
            internal static void Translate(XElement node, string translation)
            {
                XElement tmp;
                lock (_lockObject)
                {
                    _sb.Clear().Append("<R>").Append(translation).Append("</R>");
                    tmp = XElement.Parse(_sb.ToString(), LoadOptions.None);
                }

                node.ReplaceNodes(tmp.Nodes());

                if (node is XCodeCloneElement clone)
                {
                    XElement[] sourceCodeNodes = clone.Source.Elements(Sandcastle.CODE).ToArray();
                    XElement[] nodeCodeNodes = node.Elements(Sandcastle.CODE).ToArray();

                    int end = Math.Min(sourceCodeNodes.Length, nodeCodeNodes.Length);

                    for (int i = 0; i < end; i++)
                    {
                        nodeCodeNodes[i].ReplaceWith(sourceCodeNodes[i]);
                    }

                    clone.Source.ReplaceNodes(clone.Nodes());
                }
            }


            [return: NotNullIfNotNull("node")]
            internal static XElement? MaskCodeBlock(XElement? node)
            {
                if (node is null)
                {
                    return null;
                }

                if (node.Element(Sandcastle.CODE) != null)
                {
                    return new XCodeCloneElement(node);
                }
                else
                {
                    return node;
                }
            }



            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static long GetNodeID(XElement node)
            {
                int nodePathHash = GetNodePathHash(node);
                int contentHash = GetContentHash(node, out _);
                return ComputeNodeID(nodePathHash, contentHash);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static long GetNodeID(XElement node, out string innerXml, out string nodePath)
            {
                nodePath = GetNodePath(node);
                int nodePathHash = GetNodePathHash(nodePath);
                int contentHash = GetContentHash(node, out innerXml);
                return ComputeNodeID(nodePathHash, contentHash);
            }


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static string GetNodePath(XElement? node)
            {
                lock (_lockObject)
                {
                    FillStringBuilder(node);
                    return _sb.ToString();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static long ComputeNodeID(int nodePathHash, int contentHash)
            {
                long id = (uint)nodePathHash;

                id <<= 32;

                id |= (uint)contentHash;

                return id;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetNodePathHash(XElement? node)
            {
                lock (_lockObject)
                {
                    FillStringBuilder(node);
                    return _sb.GetStableHashCode(HashType.Ordinal);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetNodePathHash(string nodePath) => nodePath.GetStableHashCode(HashType.Ordinal);


            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static int GetContentHash(XElement node, out string innerXml)
            {
                innerXml = node.InnerXml();
                return innerXml.GetStableHashCode(HashType.AlphaNumericNoCase);
            }


            private static void FillStringBuilder(XElement? node)
            {
                _list.Clear();
                _sb.Clear();

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

                            _sb.Append(memberName, 2, length - 2);
                        }
                        else
                        {
                            _sb.Append(memberName);
                        }

                    }
                    else
                    {
                        _sb.Append(_list[i]);
                    }

                    if (i != 0)
                    {
                        _sb.Append('/');
                    }
                }
            }


            internal static bool IsTranslatable(string xElementName)
            {
                switch (xElementName)
                {
                    case Sandcastle.TOKEN:
                    case Sandcastle.INHERITDOC:
                    case Sandcastle.INCLUDE:
                    case Sandcastle.FILTERPRIORITY:
                    case Sandcastle.EXCLUDE:
                    case Sandcastle.THREADSAFETY:
                    case Sandcastle.CODE:
                        return false;
                    default:
                        return true;
                }
            }


            internal static bool IsContainerSection(string xElementName)
            {
                switch (xElementName)
                {
                    case Sandcastle.OVERLOADS:
                    case Sandcastle.ATTACHED_PROPERTY_COMMENTS:
                    case Sandcastle.ATTACHED_EVENT_COMMENTS:
                    case Sandcastle.REVISION_HISTORY:
                    case Sandcastle.MEMBER:
                        return true;
                    default:
                        return false;
                }
            }

        }
    }
}
