using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal sealed class ManualTranslation
    {
        internal const string XML_NAME = "MT";
        private const string ELEMENT = "Node";


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="translatedText"></param>
        internal ManualTranslation(XElement node, string translatedText)
        {
            this.Node = Utility.Instance.GetNodeHash(node);
            this.Translation = new Translation(node, translatedText);
        }


        /// <summary>
        /// ctor for Deserialization
        /// </summary>
        /// <param name="elementHash"></param>
        private ManualTranslation(int elementHash, Translation trans)
        {
            this.Node = elementHash;
            this.Translation = trans;
        }


        internal int Node { get; }

        internal Translation Translation { get; }

        


        internal void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(XML_NAME);
            writer.WriteAttributeString(ELEMENT, Node.ToString("X", CultureInfo.InvariantCulture));
            writer.WriteAttributeString(Translation.HASH, Translation.Hash.ToString("X", CultureInfo.InvariantCulture));
            writer.WriteString(Translation.Value);

            writer.WriteEndElement();
        }


        internal static ManualTranslation ParseXml(XElement el)
        {
            int elementHash = int.Parse(el.Attribute(ELEMENT).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            return new ManualTranslation(elementHash, Translation.ParseXml(el));
        }

    }
}