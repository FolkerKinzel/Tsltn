using System;
using System.Diagnostics;
using System.Xml.Linq;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal static class XElementExtensions
    {
        public static string InnerXml(this XElement node)
        {
            //if(node is null)
            //{
            //    throw new ArgumentNullException(nameof(node));
            //}

            Debug.Assert(node != null);

            var reader = node.CreateReader();
            reader.MoveToContent();

            return reader.ReadInnerXml();
        }
    }
}
