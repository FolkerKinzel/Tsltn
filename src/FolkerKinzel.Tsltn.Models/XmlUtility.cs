using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models;

public static class XmlUtility
{
    public static bool IsValidXml(string innerXml, [NotNullWhen(false)] out string? exceptionMessage)
    {
        exceptionMessage = null;

        try
        {
            _ = XElement.Parse(string.Concat("<R>", innerXml, "</R>"), LoadOptions.None);
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

    internal static bool ContainsPathFragment(string nodePath, string pathFragment, bool ignoreCase, bool wholeWord)
    {
        if (wholeWord)
        {
            const string wordBoundary = @"\b";
            string regex;
            regex = string.Concat(wordBoundary, Regex.Escape(pathFragment), wordBoundary);

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

    [SuppressMessage("Style", "IDE0066:Switch-Anweisung in Ausdruck konvertieren", Justification = "<Ausstehend>")]
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

    [SuppressMessage("Style", "IDE0066:Switch-Anweisung in Ausdruck konvertieren", Justification = "<Ausstehend>")]
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
