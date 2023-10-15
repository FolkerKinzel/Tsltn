// See https://aka.ms/new-console-template for more information

using System.Xml.Linq;

namespace Reversers;

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

