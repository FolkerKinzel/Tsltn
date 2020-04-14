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
    public class Document
    {
        private static TsltnFile? _tsltn;
        private static XDocument? _xmlDocument;

        /// <summary>
        /// ctor
        /// </summary>
        private Document() { }

        public static Document Instance { get; } = new Document();



        public bool Changed => _tsltn?.Changed ?? false;


        public string? TsltnFileName { get; private set; }


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

        public Node? FirstNode { get; private set; }

        public bool SourceDocumentExists { get; private set; }

        

        public void CreateNew(string fileToTranslate)
        {
            this.SourceDocumentExists = false;

            if (Changed)
            {
                throw new InvalidOperationException();
            }

            _tsltn = new TsltnFile
            {
                SourceDocumentFileName = fileToTranslate
            };

            if (!File.Exists(fileToTranslate))
            {
                return;
            }
            else
            {
                this.SourceDocumentExists = true;
            }

            _xmlDocument = XDocument.Load(fileToTranslate, LoadOptions.None);

            InitFirstNode();
        }


        public void Open(string? tsltnFileName)
        {
            this.SourceDocumentExists = false;

            if (Changed)
            {
                throw new InvalidOperationException();
            }

            if (tsltnFileName is null)
            {
                return;
            }

            this.TsltnFileName = tsltnFileName;

            _tsltn = TsltnFile.Load(tsltnFileName);


            string xmlFileName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(tsltnFileName), _tsltn.SourceDocumentFileName));

            if(!File.Exists(xmlFileName))
            {
                return;
            }
            else
            {
                this.SourceDocumentExists = true;
            }

            _xmlDocument = XDocument.Load(xmlFileName, LoadOptions.None);

            InitFirstNode();
        }

        private void InitFirstNode()
        {
            XElement? firstNode = GetFirstNode();

            if(firstNode is null)
            {
                FirstNode = null;
                return;
            }

            FirstNode = new Node(firstNode);
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

            _tsltn.Save(TsltnFileName);
        }


        public void Translate(string fileName)
        {
            this.Save();

            var node = GetFirstNode();

            while (node != null)
            {
                Utility.Translate(node, GetTranslation(node));
                node = GetNextNode(node);
            }

            _xmlDocument?.Save(fileName);

            this.Open(this.TsltnFileName);
        }


        public void CloseDocument()
        {
            _tsltn = null;
            this.TsltnFileName = null;
        }


        internal static XElement? GetFirstNode()
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


        private static XElement? ExtractFirstNode(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            if (Utility.IsContainerSection(section))
            {
                return Utility.MaskCodeBlock(ExtractFirstFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.MaskCodeBlock(section);
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

        
        


        internal static XElement? GetNextNode(XElement? currentNode)
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


        internal static XElement? GetPreviousNode(XElement? currentNode)
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


        


        private static XElement? ExtractLastNode(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            if (Utility.IsContainerSection(section))
            {
                return Utility.MaskCodeBlock(ExtractLastFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.MaskCodeBlock(section);
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

        

        internal static XElement? GetNextUntranslated(XElement node)
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




 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddAutoTranslation(XElement node, string? transl) => _tsltn?.AddAutoTranslation(node, transl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddManualTranslation(XElement node, string? transl) => _tsltn?.AddManualTranslation(node, transl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? GetTranslation(XElement node) => _tsltn?.GetTranslation(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetManualTranslation(XElement node) => _tsltn?.GetManualTranslation(node);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetAutoTranslation(XElement node) => _tsltn?.GetAutoTranslation(node);




    }
}
