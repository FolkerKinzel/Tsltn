using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public class Document : ITsltnFile
    {
        private TsltnFile? _tsltn;
        private XDocument? _xmlDocument;

        /// <summary>
        /// ctor
        /// </summary>
        private Document() { }


        public string? TsltnFileName { get; private set; }


        public bool Changed => _tsltn?.Changed ?? false;


        public static Document Instance { get; } = new Document();


        public bool SourceDocumentExists { get; private set; }

        

        public void CreateNew(string fileToTranslate)
        {
            if(Changed)
            {
                throw new InvalidOperationException();
            }

            this._tsltn = new TsltnFile
            {
                SourceDocumentFileName = fileToTranslate
            };
        }


        public void Open(string? tsltnFileName)
        {
            if (Changed)
            {
                throw new InvalidOperationException();
            }

            if (tsltnFileName is null)
            {
                return;
            }

            this.TsltnFileName = tsltnFileName;

            this._tsltn = TsltnFile.Load(tsltnFileName);


            string xmlFileName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(tsltnFileName), _tsltn.SourceDocumentFileName));

            if(!File.Exists(xmlFileName))
            {
                this.SourceDocumentExists = false;
                return;
            }
            else
            {
                this.SourceDocumentExists = true;
            }

            this._xmlDocument = XDocument.Load(xmlFileName, LoadOptions.None);
        }


        public void Reload()
        {
            this.Save();
            this.Open(this.TsltnFileName);
        }


        public void SaveAs(string tsltnFileName)
        {
            this.TsltnFileName = tsltnFileName;
            Save();
        }



        public void Save()
        {
            if(_tsltn is null)
            {
                return;
            }

            this._tsltn.Save(TsltnFileName);
        }


        public void CloseDocument()
        {
            this._tsltn = null;
            this.TsltnFileName = null;
        }


        public XElement? GetFirstNode()
        {
            var members = _xmlDocument?.Root.Element(Sandcastle.MEMBERS)?.Elements(Sandcastle.MEMBER);

            if (members is null)
            {
                return null;
            }

            foreach (var member in members)
            {
                foreach (XElement section in member.Elements())
                {
                    var el = ExtractFirstNode(section);

                    if (el != null) return el;
                }
            }

            return null;
        }


        private XElement? ExtractFirstNode(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            if (Utility.IsContainerSection(section))
            {
                return Utility.Instance.MaskCodeBlock(ExtractFirstFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.Instance.MaskCodeBlock(section);
            }

            return null;

            /////////////////////////////////////////////////////////////

            static XElement? ExtractFirstFromContainer(XElement container)
            {
                foreach (var innerElement in container.Elements())
                {
                    if (Utility.IsContainerSection(innerElement))
                    {
                        return ExtractFirstFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElement))
                    {
                        return innerElement;
                    }
                }

                return null;
            }
        }

        
        


        public XElement? GetNextNode(XElement? currentNode)
        {
            while (true)
            {
                if(currentNode is XCodeCloneElement clone)
                {
                    currentNode = clone.Source;
                }

                if (currentNode is null || currentNode.Name == Sandcastle.MEMBERS)
                {
                    break;
                }

                foreach (var sibling in currentNode.ElementsAfterSelf())
                {
                    var el = ExtractFirstNode(sibling);

                    if (el != null)
                    {
                        return el;
                    }
                }

                currentNode = currentNode.Parent;
            }

            return null;
        }


        public XElement? GetPreviousNode(XElement? currentNode)
        {
            while (true)
            {
                if (currentNode is XCodeCloneElement clone)
                {
                    currentNode = clone.Source;
                }

                if (currentNode is null || currentNode.Name == Sandcastle.MEMBERS)
                {
                    break;
                }

                foreach (var sibling in currentNode.ElementsBeforeSelf().Reverse())
                {
                    var el = ExtractLastNode(sibling);

                    if (el != null)
                    {
                        return el;
                    }
                }

                currentNode = currentNode.Parent;
            }

            return null;
        }


        


        private XElement? ExtractLastNode(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            if (Utility.IsContainerSection(section))
            {
                return Utility.Instance.MaskCodeBlock(ExtractLastFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.Instance.MaskCodeBlock(section);
            }

            return null;

            ///////////////////////////////////////////////////////////////

            static XElement? ExtractLastFromContainer(XElement container)
            {
                foreach (var innerElement in container.Elements().Reverse())
                {
                    if (Utility.IsContainerSection(innerElement))
                    {
                        return ExtractLastFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElement))
                    {
                        return innerElement;
                    }
                }

                return null;
            }
        }

        
        public void Translate(string fileName)
        {
            this.Save();

            var util = Utility.Instance;
            var node = GetFirstNode();

            while(node != null)
            {
                util.Translate(node, GetTranslation(node));
                node = GetNextNode(node);
            }

            _xmlDocument?.Save(fileName);

            this.Open(this.TsltnFileName);
        }


        public XElement? FindNextUntranslated(XElement node)
        {
            XElement? unTrans = GetNextNode(node);
            while(unTrans != null)
            {
                if(GetTranslation(unTrans) is null)
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
            }

            unTrans = GetFirstNode();

            while(unTrans != null && !object.ReferenceEquals(unTrans, node))
            {
                if (GetTranslation(unTrans) is null)
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
            }

            return null;
        }



        #region ITsltnFile

        public string? SourceDocumentFileName 
        {
            get => _tsltn?.SourceDocumentFileName;
            set 
            { 
                if (_tsltn != null) 
                { 
                    _tsltn.SourceDocumentFileName = value;
                } 
            }
        }

        public string? SourceLanguage 
        { 
            get => _tsltn?.SourceLanguage; 
            set 
            { 
                if (_tsltn != null) 
                { 
                    _tsltn.SourceLanguage = value;
                } 
            } 
        }

        public string? TargetLanguage
        { 
            get => _tsltn?.TargetLanguage;
            set 
            {
                if (_tsltn != null)
                {
                    _tsltn.TargetLanguage = value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAutoTranslation(XElement node, string translatedText) => _tsltn?.AddAutoTranslation(node, translatedText);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddManualTranslation(XElement node, string translatedText) => _tsltn?.AddManualTranslation(node, translatedText);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetTranslation(XElement node) => _tsltn?.GetTranslation(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveManualTranslation(XElement node) => _tsltn?.RemoveManualTranslation(node);

        #endregion

    
    }
}
