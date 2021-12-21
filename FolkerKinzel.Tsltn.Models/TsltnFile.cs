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
using System.ComponentModel;

namespace FolkerKinzel.Tsltn.Models
{
    [XmlRoot("Tsltn")]
    public sealed class TsltnFile : IXmlSerializable //, INotifyPropertyChanged
    {
        private const string FILE_VERSION = "1.0";

        private const string TRANSLATION_XML_NAME = "T";
        private const string ID = "ID";


        private const string FILE_VERSION_XML_STRING = "Version";
        private const string SOURCE_LANGUAGE = "SourceLanguage";
        private const string TARGET_LANGUAGE = "TargetLanguage";
        private const string SOURCE_FILE = "SourceFile";

        private readonly Dictionary<long, string> _translations = new Dictionary<long, string>();

        private string? _sourceDocumentRelativePath;
        private string? _sourceDocumentAbsolutePath;
        private string? _sourceLanguage;
        private string? _targetLanguage;
        private bool _changed;

        //public event PropertyChangedEventHandler? PropertyChanged;
        //private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


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

                _sourceDocumentRelativePath = _sourceDocumentAbsolutePath = string.IsNullOrWhiteSpace(value) ? null : value;
                //OnPropertyChanged();
                Changed = true;
            }
        }


        internal string? SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            {
                _sourceLanguage = value;
                //OnPropertyChanged();
                Changed = true;
            }
        }


        internal string? TargetLanguage
        {
            get { return _targetLanguage; }
            set
            {
                _targetLanguage = value;
                //OnPropertyChanged();
                Changed = true;
            }
        }


        internal bool Changed
        {
            get
            {
                return _changed;
            }

            private set
            {
                _changed = value;
                //OnPropertyChanged();
            }
        }


        internal void SetTranslation(long nodeID, string? transl)
        {
            lock (_translations)
            {
                if (transl is null)
                {
                    if (_translations.Remove(nodeID))
                    {
                        Changed = true;
                    }
                }
                else
                {
                    _translations[nodeID] = transl;
                    Changed = true;
                }
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl)
        {
            lock (_translations)
            {
                return _translations.TryGetValue(nodeID, out transl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasTranslation(long nodeID)
        {
            lock (_translations) 
            { 
                return _translations.ContainsKey(nodeID); 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IEnumerable<KeyValuePair<long, string>> GetAllTranslations()
        {
            lock (_translations)
            {
                return _translations.ToArray();
            }
        }

        internal void Save(string? tsltnFileName)
        {
            if (Path.IsPathRooted(_sourceDocumentRelativePath))
            {
                _sourceDocumentRelativePath = Path.GetRelativePath(Path.GetDirectoryName(tsltnFileName), _sourceDocumentRelativePath);
            }

            var settings = new XmlWriterSettings
            {
                Indent = true
            };


            using var writer = XmlWriter.Create(tsltnFileName, settings);

            var serializer = new XmlSerializer(typeof(TsltnFile));
            serializer.Serialize(writer, this);
           
            Changed = false;
        }

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

        public XmlSchema? GetSchema() => null;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Argumente von öffentlichen Methoden validieren", Justification = "<Ausstehend>")]
        public void ReadXml(XmlReader reader)
        {
            _ = reader.MoveToContent();

            _sourceDocumentRelativePath = reader.GetAttribute(SOURCE_FILE);

            //if(_sourceDocumentRelativePath is null)
            //{
            //    throw new XmlException("Source document filename is missing.");
            //}

            _sourceLanguage = reader.GetAttribute(SOURCE_LANGUAGE);
            _targetLanguage = reader.GetAttribute(TARGET_LANGUAGE);

            _ = reader.Read();

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
                                    SetTranslation(id, el.Value);
                                }
                            }
                            break;
                        default:
                            _ = reader.Read();
                            break;
                    }
                }
                else
                {
                    _ = reader.Read();
                }
            }//while

            Changed = false;
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

            lock (_translations)
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
