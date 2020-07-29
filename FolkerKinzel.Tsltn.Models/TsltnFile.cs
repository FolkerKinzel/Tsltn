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
using System.Linq;

namespace FolkerKinzel.Tsltn.Models
{
    [XmlRoot("Tsltn")]
    public sealed class TsltnFile : IXmlSerializable
    {
        private const string FILE_VERSION = "1.0";

        internal const string TRANSLATION_XML_NAME = "T";
        internal const string ID = "ID";


        private const string FILE_VERSION_XML_STRING = "Version";
        private const string SOURCE_LANGUAGE = "SourceLanguage";
        private const string TARGET_LANGUAGE = "TargetLanguage";
        private const string SOURCE_FILE = "SourceFile";

        private readonly Dictionary<long, string> _translations = new Dictionary<long, string>();

        private string? _sourceDocumentRelativePath;
        private string? _sourceDocumentAbsolutePath;
        private string? _sourceLanguage;
        private string? _targetLanguage;

        /// <summary>
        /// ctor
        /// </summary>
        public TsltnFile() { }


        internal string? SourceDocumentFileName
        {
            get { return _sourceDocumentAbsolutePath; }
            set
            {
                // Der relative Pfad wird beim Speichern wieder in einen solchen 
                // umgewandelt. In der Anwendung wird nur der absolute Pfad benutzt.

                _sourceDocumentRelativePath = _sourceDocumentAbsolutePath = value;
                Changed = true;
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
            lock (this._translations)
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
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl)
        {
            lock (this._translations)
            {
                return _translations.TryGetValue(nodeID, out transl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasTranslation(long nodeID)
        {
            lock (this._translations) 
            { 
                return _translations.ContainsKey(nodeID); 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<KeyValuePair<long, string>> GetAllTranslations()
        {
            lock (this._translations)
            {
                return this._translations.ToArray();
            }
        }

        internal void Save(string? tsltnFileName)
        {
            if (Path.IsPathRooted(this._sourceDocumentRelativePath))
            {
                this._sourceDocumentRelativePath = Path.GetRelativePath(Path.GetDirectoryName(tsltnFileName), this._sourceDocumentRelativePath);
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


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        internal static TsltnFile Load(string tsltnFileName)
        {
            using var reader = XmlReader.Create(tsltnFileName);

            var serializer = new XmlSerializer(typeof(TsltnFile));

            var tsltn = (TsltnFile)serializer.Deserialize(reader);

            try
            {
                tsltn._sourceDocumentAbsolutePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(tsltnFileName), tsltn._sourceDocumentRelativePath));
            }
            catch { }

            return tsltn;
        }


        #region IXmlSerializable

        public XmlSchema? GetSchema()
        {
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Argumente von öffentlichen Methoden validieren", Justification = "<Ausstehend>")]
        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            _sourceDocumentRelativePath = reader.GetAttribute(SOURCE_FILE);

            if(_sourceDocumentRelativePath is null)
            {
                throw new XmlException("Source document filename is missing.");
            }

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

            writer.WriteAttributeString(SOURCE_FILE, _sourceDocumentRelativePath);

            string? sourceLanguage = SourceLanguage;
            if (sourceLanguage != null)
            {
                writer.WriteAttributeString(SOURCE_LANGUAGE, sourceLanguage);
            }

            string? targetLanguage = TargetLanguage;
            if (targetLanguage != null)
            {
                writer.WriteAttributeString(TARGET_LANGUAGE, targetLanguage);
            }

            lock (this._translations)
            {
                foreach (KeyValuePair<long, string> kvp in _translations)
                {
                    writer.WriteStartElement(TRANSLATION_XML_NAME);
                    writer.WriteAttributeString(ID, kvp.Key.ToString("X", CultureInfo.InvariantCulture));
                    writer.WriteString(kvp.Value);
                    writer.WriteEndElement();
                }
            }
        }

        #endregion
    }
}
