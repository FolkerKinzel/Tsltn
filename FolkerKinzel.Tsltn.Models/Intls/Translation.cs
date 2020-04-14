using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace FolkerKinzel.Tsltn.Models.Intls
{
    internal sealed class Translation
    {
        internal const string TRANSLATION_XML_NAME = "T";
        internal const string HASH = "Hash";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="node"></param>
        /// <param name="translatedText"></param>
        public Translation(XElement node, string translatedText)
        {
            this.Hash = Utility.Instance.GetOriginalTextHash(node);
            this.Value = translatedText;
        }

        /// <summary>
        /// ctor for Deserialization
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="value"></param>
        private Translation(int hash, string value)
        {
            this.Hash = hash;
            this.Value = value;
        }

        internal int Hash { get; }

        public string? Value { get; }

        //public bool IsEmpty => string.IsNullOrWhiteSpace(this.Value);


        #region Serialization

        internal void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(TRANSLATION_XML_NAME);
            writer.WriteAttributeString(HASH, Hash.ToString("X", CultureInfo.InvariantCulture));
            writer.WriteString(Value);
            writer.WriteEndElement();
        }

        internal static Translation ParseXml(XElement el)
        {
            int hash = int.Parse(el.Attribute(HASH).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
            return new Translation(hash, el.Value);
        }

        #endregion
    }
}