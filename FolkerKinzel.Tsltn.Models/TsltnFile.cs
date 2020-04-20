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
using System.Diagnostics.CodeAnalysis;

namespace FolkerKinzel.Tsltn.Models
{
    [XmlRoot("Tsltn")]
    public sealed class TsltnFile : IXmlSerializable
    {
        private const string FILE_VERSION = "1.0";

        //private const string MANUAL_TRANSLATION_XML_NAME = "MT";
        //private const string ELEMENT = "Node";

        internal const string TRANSLATION_XML_NAME = "T";
        //internal const string HASH = "Hash";
        internal const string ID = "ID";


        private const string FILE_VERSION_XML_STRING = "Version";
        private const string SOURCE_LANGUAGE = "SourceLanguage";
        private const string TARGET_LANGUAGE = "TargetLanguage";
        private const string SOURCE_FILE = "SourceFile";


        //private readonly Dictionary<int, string> _autoTranslations = new Dictionary<int, string>();
        private readonly Dictionary<long, string> _translations = new Dictionary<long, string>();

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

        
        internal void SetTranslation(long nodeID, string? transl)
        {
            if (transl is null)
            {
                if (this._translations.Remove(nodeID))
                {
                    Changed = true;
                }
            }
            else
            {
                this._translations[nodeID] = transl;
                Changed = true;
            }
        }

 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl) => _translations.TryGetValue(nodeID, out transl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasTranslation(long nodeID) => _translations.ContainsKey(nodeID);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<KeyValuePair<long, string>> GetAllTranslations() => this._translations;
        



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
                                    long id = long.Parse(el.Attribute(ID).Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                                    this.SetTranslation(id, el.Value);
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

            foreach (KeyValuePair<long, string> kvp in _translations)
            {
                writer.WriteStartElement(TRANSLATION_XML_NAME);
                writer.WriteAttributeString(ID, kvp.Key.ToString("X", CultureInfo.InvariantCulture));
                writer.WriteString(kvp.Value);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
