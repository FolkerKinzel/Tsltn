using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.Linq;
using System.Globalization;
using System.Text;
using System.IO;

namespace FolkerKinzel.Tsltn.Models
{
    [XmlRoot("Tsltn")]
    public sealed class TsltnFile : IXmlSerializable, ITsltnFile
    {
        private const string FILE_VERSION = "1.0";

        private const string FILE_VERSION_XML_STRING = "Version";
        private const string SOURCE_LANGUAGE = "SourceLanguage";
        private const string TARGET_LANGUAGE = "TargetLanguage";
        private const string SOURCE_FILE = "SourceFile";


        private readonly Dictionary<int, Translation> _autoTranslations = new Dictionary<int, Translation>();
        private readonly Dictionary<int, ManualTranslation> _manualTranslations = new Dictionary<int, ManualTranslation>();

        private string? _sourceDocumentFileName;
        private string? _sourceLanguage;
        private string? _targetLanguage;


        public TsltnFile() { }

        public string? SourceDocumentFileName
        {
            get { return _sourceDocumentFileName; }
            set 
            { 
                _sourceDocumentFileName = value;
                this.Changed = true;
            }
        }


        public string? SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            {
                _sourceLanguage = value;
                this.Changed = true;
            }
        }


        public string? TargetLanguage
        {
            get { return _targetLanguage; }
            set
            { 
                _targetLanguage = value;
                this.Changed = true;
            }
        }


        public bool Changed { get; private set; }


        public void AddManualTranslation(XText node, string translatedText)
        {
            var manual = new ManualTranslation(node, translatedText);

            AddManualTranslation(manual);

            Changed = true;
        }


        private void AddManualTranslation(ManualTranslation manual)
        {
            if (manual.IsEmpty) return;

            if (this._manualTranslations.ContainsKey(manual.Node))
            {
                this._manualTranslations[manual.Node].Merge(manual);
            }
            else
            {
                this._manualTranslations[manual.Node] = manual;
            }
        }


        public void RemoveManualTranslation(XText node)
        {
            int elementHash = Utility.Instance.GetNodeHash(node);
            int originalTextHash = HashService.HashOriginalText(node.Value);

            if (_manualTranslations.ContainsKey(elementHash))
            {
                var mt = _manualTranslations[elementHash];

                if (mt.RemoveTranslation(originalTextHash))
                {
                    Changed = true;
                }
            }
        }

        public void AddAutoTranslation(XText node, string translatedText)
        {
            var transl = new Translation(node, translatedText);

            AddAutoTranslation(transl);

            Changed = true;
        }


        private void AddAutoTranslation(Translation transl)
        {
            if (transl.IsEmpty)
            {
                this._autoTranslations.Remove(transl.Hash);
            }
            else
            {
                this._autoTranslations[transl.Hash] = transl;
            }
        }


        public string? GetTranslation(XText node)
        {
            int elementHash = Utility.Instance.GetNodeHash(node);
            int originalTextHash = HashService.HashOriginalText(node.Value);

            Translation? translation = null;

            if (_manualTranslations.ContainsKey(elementHash))
            {
                translation = _manualTranslations[elementHash].GetTranslation(originalTextHash);
            }

            if (translation is null)
            {
                if (_autoTranslations.ContainsKey(originalTextHash))
                {
                    return _autoTranslations[originalTextHash].Value;
                }
            }

            return translation?.Value;
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


        public void ReadXml(XmlReader reader)
        {
            //var xElement = XElement.Load(reader);

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
                        case Translation.XML_NAME:
                            {
                                if (XNode.ReadFrom(reader) is XElement el)
                                {
                                    this.AddAutoTranslation(Translation.ParseXml(el));
                                }
                            }
                            break;
                        case ManualTranslation.XML_NAME:
                            {
                                if (XNode.ReadFrom(reader) is XElement el)
                                {
                                    this.AddManualTranslation(ManualTranslation.ParseXml(el));
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
            }
        }




        public void WriteXml(XmlWriter writer)
        {
            //writer.WriteStartElement(XML_NAME);

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



            foreach (KeyValuePair<int, Translation> kvp in _autoTranslations)
            {
                kvp.Value.WriteXml(writer);
            }


            foreach (KeyValuePair<int, ManualTranslation> kvp in _manualTranslations)
            {
                var trans = kvp.Value;

                if (!trans.IsEmpty)
                {
                    trans.WriteXml(writer);
                }
            }
        }

        #endregion
    }
}
