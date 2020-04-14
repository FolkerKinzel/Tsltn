using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Globalization;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models
{
    [XmlRoot("Tsltn")]
    public sealed class TsltnFile : IXmlSerializable
    {
        private const string FILE_VERSION = "1.0";

        private const string MANUAL_TRANSLATION_XML_NAME = "MT";
        private const string ELEMENT = "Node";

        internal const string TRANSLATION_XML_NAME = "T";
        internal const string HASH = "Hash";

        private const string FILE_VERSION_XML_STRING = "Version";
        private const string SOURCE_LANGUAGE = "SourceLanguage";
        private const string TARGET_LANGUAGE = "TargetLanguage";
        private const string SOURCE_FILE = "SourceFile";


        private readonly Dictionary<int, string> _autoTranslations = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _manualTranslations = new Dictionary<int, string>();

        private string? _sourceDocumentFileName;
        private string? _sourceLanguage;
        private string? _targetLanguage;


        public TsltnFile() { }

        internal string? SourceDocumentFileName
        {
            get { return _sourceDocumentFileName; }
            set 
            { 
                _sourceDocumentFileName = value;
                this.Changed = true;
            }
        }


        internal string? SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            {
                _sourceLanguage = value;
                this.Changed = true;
            }
        }


        internal string? TargetLanguage
        {
            get { return _targetLanguage; }
            set
            { 
                _targetLanguage = value;
                this.Changed = true;
            }
        }


        internal bool Changed { get; private set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddManualTranslation(XElement node, string? transl)
        {
            AddManualTranslation(Utility.GetNodeHash(node), transl);
        }

        
        private void AddManualTranslation(int nodeHash, string? transl)
        {
            if (transl is null)
            {
                if (this._manualTranslations.Remove(nodeHash))
                {
                    Changed = true;
                }
            }
            else
            {
                this._manualTranslations[nodeHash] = transl;
                Changed = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddAutoTranslation(XElement node, string? transl)
        {
            AddAutoTranslation(Utility.GetOriginalTextHash(node), transl);
        }

        private void AddAutoTranslation(int hash, string? transl)
        {
            if (transl is null)
            {
                if (this._autoTranslations.Remove(hash))
                {
                    Changed = true;
                }
            }
            else
            {
                this._autoTranslations[hash] = transl;

                Changed = true;
            }
        }

        internal string? GetTranslation(XElement node)
        {
            int elementHash = Utility.GetNodeHash(node);

            if (_manualTranslations.ContainsKey(elementHash))
            {
                return _manualTranslations[elementHash];
            }

            int originalTextHash = Utility.GetOriginalTextHash(node);

            if (_autoTranslations.ContainsKey(originalTextHash))
            {
                return _autoTranslations[originalTextHash];
            }
            
            return null;
        }

     
        internal string? GetManualTranslation(XElement node)
        {
            int nodeHash = Utility.GetNodeHash(node);

            return _manualTranslations.ContainsKey(nodeHash) ? _manualTranslations[nodeHash] : null;
        }

    
        internal string? GetAutoTranslation(XElement node)
        {
            int hash = Utility.GetOriginalTextHash(node);

            return _autoTranslations.ContainsKey(hash) ? _autoTranslations[hash] : null;
        }

        internal void Save(string? tsltnFileName)
        {

            if (this.SourceDocumentFileName != null)
            {
                if (Path.IsPathRooted(this.SourceDocumentFileName))
                {
                    this.SourceDocumentFileName = Path.GetRelativePath(Path.GetDirectoryName(tsltnFileName), this.SourceDocumentFileName);
                }
            }


            var settings = new XmlWriterSettings
            {
                Indent = true
            };


            using var writer = XmlWriter.Create(tsltnFileName, settings);
            var serializer = new XmlSerializer(typeof(TsltnFile));

            serializer.Serialize(writer, this);

            this.Changed = false;
        }


        internal static TsltnFile Load(string fileName)
        {
            using var reader = XmlReader.Create(fileName);

            var serializer = new XmlSerializer(typeof(TsltnFile));

            return (TsltnFile)serializer.Deserialize(reader);
        }


        #region IXmlSerializable

        public XmlSchema? GetSchema()
        {
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Argumente von öffentlichen Methoden validieren", Justification = "<Ausstehend>")]
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            _sourceDocumentFileName = reader.GetAttribute(SOURCE_FILE);
            _sourceLanguage = reader.GetAttribute(SOURCE_LANGUAGE);
            _targetLanguage = reader.GetAttribute(TARGET_LANGUAGE);

            reader.Read();

            while (!reader.EOF && reader.ReadState == ReadState.Interactive)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case TRANSLATION_XML_NAME:
                            {
                                if (XNode.ReadFrom(reader) is XElement el)
                                {
                                    int hash = int.Parse(el.Attribute(HASH).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                    this.AddAutoTranslation(hash, el.Value);
                                }
                            }
                            break;
                        case MANUAL_TRANSLATION_XML_NAME:
                            {
                                if (XNode.ReadFrom(reader) is XElement el)
                                {
                                    int elementHash = int.Parse(el.Attribute(ELEMENT).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                    this.AddManualTranslation(elementHash, el.Value);
                                }
                            }
                            break;
                        default:
                            reader.Read();
                            break;
                    }
                }
                else
                {
                    reader.Read();
                }
            }//while

            this.Changed = false;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Argumente von öffentlichen Methoden validieren", Justification = "<Ausstehend>")]
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(FILE_VERSION_XML_STRING, FILE_VERSION);

            if (SourceDocumentFileName != null)
            {
                writer.WriteAttributeString(SOURCE_FILE, SourceDocumentFileName);
            }

            if (SourceLanguage != null)
            {
                writer.WriteAttributeString(SOURCE_LANGUAGE, SourceLanguage);
            }

            if (TargetLanguage != null)
            {
                writer.WriteAttributeString(TARGET_LANGUAGE, TargetLanguage);
            }

            foreach (KeyValuePair<int, string> kvp in _autoTranslations)
            {
                writer.WriteStartElement(TRANSLATION_XML_NAME);
                writer.WriteAttributeString(HASH, kvp.Key.ToString("X", CultureInfo.InvariantCulture));
                writer.WriteString(kvp.Value);
                writer.WriteEndElement();
            }


            foreach (KeyValuePair<int, string> kvp in _manualTranslations)
            {
                writer.WriteStartElement(MANUAL_TRANSLATION_XML_NAME);
                writer.WriteAttributeString(ELEMENT, kvp.Key.ToString("X", CultureInfo.InvariantCulture));
                writer.WriteString(kvp.Value);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
