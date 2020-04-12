using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public class Document : ITsltnFile //: INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;

        //private Node? _currentNode;

        private const string MEMBERS = "members";
        private const string MEMBER = "member";

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


        public void Open(string tsltnFileName)
        {
            if (Changed)
            {
                throw new InvalidOperationException();
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
            this.Open(this.TsltnFileName!);
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


        public XText? GetFirstTextNode()
        {
            var members = _xmlDocument?.Root.Element(MEMBERS)?.Elements(MEMBER);

            if (members is null)
            {
                return null;
            }

            foreach (var member in members)
            {
                foreach (var section in member.Elements())
                {
                    foreach (var node in section.DescendantNodes())
                    {
                        var extractedNode = ExtractFirstNode(node);

                        if(extractedNode is XText txt)
                        {
                            return txt;
                        }
                    }
                }
            }

            return null;
        }


        private XNode ExtractFirstNode(XNode parent)
        {
            if(parent is XElement el)
            {
                XNode firstChild = el.FirstNode;

                if(firstChild is XElement el2)
                {
                    return ExtractFirstNode(el2);
                }
                else if(firstChild is null)
                {
                    return parent;
                }
                else
                {
                    return firstChild;
                }
            }
            else
            {
                return parent;
            }
        }


        


        public XNode? GetNextNode(XNode currentNode)
        {
            XNode? nextNode = currentNode.NextNode;

            if (nextNode != null)
            {
                return ExtractFirstNode(nextNode);
            }


            while (true)
            {
                XElement parent = currentNode.Parent;


                if (parent.Name == MEMBERS)
                {
                    break;
                }
                

                var sibling = parent.NextNode;

                if(sibling is null)
                {
                    currentNode = parent;
                    continue;
                }

                return ExtractFirstNode(sibling);
            }

            return null;
        }


        public XNode? GetPreviousNode(XNode currentNode)
        {
            XNode? previousNode = currentNode.PreviousNode;

            if (previousNode != null)
            {
                return ExtractLastNode(previousNode);
            }


            while (true)
            {
                XElement parent = currentNode.Parent;


                if (parent.Name == MEMBERS)
                {
                    break;
                }


                var sibling = parent.PreviousNode;

                if (sibling is null)
                {
                    currentNode = parent;
                    continue;
                }

                return ExtractLastNode(sibling);
            }

            return null;
        }

        private XNode ExtractLastNode(XNode parent)
        {
            if (parent is XElement el)
            {
                XNode lastChild = el.LastNode;

                if (lastChild is XElement el2)
                {
                    return ExtractLastNode(el2);
                }
                else if (lastChild is null)
                {
                    return parent;
                }
                else
                {
                    return lastChild;
                }
            }
            else
            {
                return parent;
            }
        }


        

        #region ITsltnFile

        public string? SourceDocumentFileName { get => _tsltn?.SourceDocumentFileName; set { if (_tsltn != null) { _tsltn.SourceDocumentFileName = value; } } }
        public string? SourceLanguage { get => _tsltn?.SourceLanguage; set { if (_tsltn != null) { _tsltn.SourceLanguage = value; } } }
        public string? TargetLanguage { get => _tsltn?.TargetLanguage; set { if (_tsltn != null) { _tsltn.TargetLanguage = value; } } }


        public void AddAutoTranslation(XText node, string translatedText) => _tsltn?.AddAutoTranslation(node, translatedText);

        public void AddManualTranslation(XText node, string translatedText) => _tsltn?.AddManualTranslation(node, translatedText);

        public string? GetTranslation(XText node) => _tsltn?.GetTranslation(node);

        public void RemoveManualTranslation(XText node) => _tsltn?.RemoveManualTranslation(node);

        #endregion

        //private void OnPropertyChanged(string propertyName) => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
