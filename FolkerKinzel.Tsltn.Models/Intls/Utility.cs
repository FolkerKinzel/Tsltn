using FolkerKinzel.Strings;
using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal static class Utility
    {
        private static readonly StringBuilder _sb = new StringBuilder();
        private static readonly List<string> _list = new List<string>();


        internal static void Translate(XElement node, string? translation)
        {
            if (translation is null)
            {
                return;
            }

            const string ROOT = "Root";
            _sb.Clear();

            _sb.Append('<').Append(ROOT).Append('>').Append(translation).Append("</").Append(ROOT).Append('>');

            var tmp = XElement.Parse(_sb.ToString(), LoadOptions.None);

            node.ReplaceNodes(tmp.Nodes());

            if (node is XCodeCloneElement clone)
            {
                var sourceCodeNodes = clone.Source.Elements(Sandcastle.CODE).ToArray();
                var nodeCodeNodes = node.Elements(Sandcastle.CODE).ToArray();

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


        internal static string GetNodePath(XElement? node)
        {
            FillStringBuilder(node);
            return _sb.ToString();
        }


        internal static int GetNodeHash(XElement? node)
        {
            FillStringBuilder(node);
            return _sb.GetStableHashCode(HashType.Ordinal);
        }


        internal static int GetOriginalTextHash(XElement node)
        {
            return node.GetInnerXml().GetStableHashCode(HashType.AlphaNumericNoCase);
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


        internal static bool IsTranslatable(XElement section)
        {
            switch (section.Name.LocalName)
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


        internal static bool IsContainerSection(XElement section)
        {
            switch (section.Name.LocalName)
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
