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
    internal static class Utility
    {
        internal static bool ContainsPathFragment(string nodePath, string pathFragment, bool ignoreCase, bool wholeWord)
        {
            if (wholeWord)
            {
                string regex;
                regex = $"\b{Regex.Escape(pathFragment)}\b";

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

        


        internal static long ComputeNodeID(int nodePathHash, int contentHash)
        {
            long id = (uint)nodePathHash;

            id <<= 32;

            id |= (uint)contentHash;

            return id;
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
