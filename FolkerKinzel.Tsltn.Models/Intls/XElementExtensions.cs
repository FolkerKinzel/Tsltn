using System;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal static class XElementExtensions
    {
        public static string GetInnerXml(this XElement node)
        {
            if(node is null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var reader = node.CreateReader();
            reader.MoveToContent();

            return reader.ReadInnerXml();
        }
    }
}
