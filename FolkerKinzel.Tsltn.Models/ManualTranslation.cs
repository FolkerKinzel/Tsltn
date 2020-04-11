using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FolkerKinzel.Tsltn.Models
{
    public sealed class ManualTranslation
    {
        internal const string XML_NAME = "MT";

        private const string ELEMENT = "Node";


        private readonly Dictionary<int, Translation> _translations = new Dictionary<int, Translation>();


        public ManualTranslation(string elementXPath)
        {
            this.Node = HashService.HashXPath(elementXPath);
        }

       

        internal ManualTranslation(int elementHash)
        {
            this.Node = elementHash;
        }


        internal int Node { get; }

        internal bool IsEmpty => _translations.Count == 0;



        public void AddTranslation(Translation transl)
        {
            if (!transl.IsEmpty)
            {
                this._translations[transl.Hash] = transl;
            }
        }


        internal void Merge(ManualTranslation other)
        {
            foreach (var kvp in other._translations)
            {
                this.AddTranslation(kvp.Value);
            }
        }


        internal Translation? GetTranslation(int hash) => _translations.ContainsKey(hash) ? _translations[hash] : null;


        internal void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(XML_NAME);
            writer.WriteAttributeString(ELEMENT, Node.ToString("X", CultureInfo.InvariantCulture));

            foreach (KeyValuePair<int, Translation> keyValuePair in _translations)
            {
                keyValuePair.Value.WriteXml(writer);
            }

            writer.WriteEndElement();
        }


        internal static ManualTranslation ParseXml(XElement el)
        {
            int elementHash = int.Parse(el.Attribute(ELEMENT).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);

            var manual = new ManualTranslation(elementHash);

            foreach (var tslNode in el.Elements())
            {
                manual.AddTranslation(Translation.ParseXml(tslNode));
            }

            return manual;
        }

        internal bool RemoveTranslation(int key) => this._translations.Remove(key);
    }
}