using System.Diagnostics;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls;

internal static class XElementExtensions
{
    public static string InnerXml(this XElement node)
    {
        Debug.Assert(node != null);

        System.Xml.XmlReader reader = node.CreateReader();
        _ = reader.MoveToContent();

        return reader.ReadInnerXml();
    }
}
