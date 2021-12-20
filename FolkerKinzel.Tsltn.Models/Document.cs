using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public partial class Document : IDocument, IFileAccess, IDocumentNodes
    {
        private readonly TsltnFile _tsltn;
        private XDocument? _xmlDocument;

        private readonly Utility _utility = new Utility();

        private readonly object _lockObject = new object();

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="tsltnFile">The <see cref="TsltnFile"/> to work with.</param>
        /// <remarks>
        /// Let this ctor be internal to make the class testable.
        /// </remarks>
        internal Document(TsltnFile tsltnFile)
        {
            _tsltn = tsltnFile;
            _tsltn.PropertyChanged += Tsltn_PropertyChanged;
        }

        private void Tsltn_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TsltnFile.Changed))
            {
                OnPropertyChanged(nameof(Changed));
            }
        }


        #region IDocument

        #region Properties




        #endregion

        #region Methods

        public static Document NewTsltn(string sourceDocumentFileName)
        {
            //CloseTsltn();

            var doc = new Document(new TsltnFile()
            {
                SourceDocumentFileName = sourceDocumentFileName
            });

            doc._xmlDocument = XDocument.Load(sourceDocumentFileName, LoadOptions.None);
            doc.InitFirstNode();

            return doc;
        }

        public static Document OpenTsltn(string? tsltnFileName, out bool sourceDocumentFound)
        {
            //CloseTsltn();

            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }


            var doc = new Document(TsltnFile.Load(tsltnFileName));
            //doc.TsltnFileName = tsltnFileName;

            sourceDocumentFound = doc.LoadSourceDocument(doc.SourceDocumentFileName);

            return doc;
        }


        private bool LoadSourceDocument(string? xmlFileName)
        {
            if (xmlFileName is null || !File.Exists(xmlFileName))
            {
                return false;
            }
            
            _xmlDocument = XDocument.Load(xmlFileName, LoadOptions.None);

            InitFirstNode();

            return true;
        }


        public bool ReloadSourceDocument(string fileName)
        {
            Debug.Assert(_tsltn != null);

            if(LoadSourceDocument(fileName))
            {
                _tsltn.SourceDocumentFileName = fileName;
                return true;
            }

            return false;
        }



        public void Save(string tsltnFileName)
        {
            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }

            _tsltn?.Save(tsltnFileName);
            //this.TsltnFileName = tsltnFileName;
        }



        public void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations)
        {
            var cloneDoc = new XDocument(_xmlDocument);
            XElement? node = GetFirstNode(cloneDoc);

            errors = new List<DataError>();
            var used = new List<KeyValuePair<long, string>>();

            while (node != null)
            {
                try
                {
                    KeyValuePair<long, string>? trans = GetTranslation(node);

                    if (trans.HasValue)
                    {
                        used.Add(trans.Value);

                        lock (_lockObject)
                        {
                            _utility.Translate(node, trans.Value.Value);
                        }
                    }
                }
                catch (XmlException e)
                {
                    errors.Add(new XmlDataError(new Node(node, this), e.Message));
                }
                node = GetNextNode(node);
            }

            cloneDoc.Save(outFileName);

            unusedTranslations = GetAllTranslations().Except(used, new KeyValuePairComparer()).ToList();
        }

        private KeyValuePair<long, string>? GetTranslation(XElement node)
        {
            long nodePathHash = GetNodeID(node);

            if (TryGetTranslation(nodePathHash, out string? manualTransl))
            {
                return new KeyValuePair<long, string>(nodePathHash, manualTransl);
            }

            return null;
        }


       


        //public void CloseTsltn()
        //{
        //    //_tsltn = null;
        //    //TsltnFileName = null;
        //    //FirstNode = null;

        //    _utility.Cleanup();
        //}

        #endregion

        #endregion

        #region internal

        internal XElement? GetFirstNode()
        {
            lock (_lockObject)
            {
                return _xmlDocument is null ? null : GetFirstNode(_xmlDocument);
            }
        }


        private static XElement? GetFirstNode(XDocument xDoc)
        {
            IEnumerable<XElement>? members = xDoc.Root.Element(Sandcastle.MEMBERS)?.Elements(Sandcastle.MEMBER);

            if (members is null)
            {
                return null;
            }

            foreach (XElement member in members)
            {
                foreach (XElement section in member.Elements())
                {
                    XElement? el = ExtractDescendant(section);

                    if (el != null)
                    {
                        return el;
                    }
                }
            }
        
            return null;
        }

        public XElement? GetNextNode(XElement? currentNode)
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

                lock (_lockObject)
                {
                    foreach (XElement sibling in currentNode.ElementsAfterSelf())
                    {
                        XElement? el = ExtractDescendant(sibling);

                        if (el != null)
                        {
                            return el;
                        }
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

                lock (_lockObject)
                {
                    foreach (XElement? sibling in currentNode.ElementsBeforeSelf().Reverse())
                    {
                        XElement? el = ExtractAncestor(sibling);

                        if (el != null)
                        {
                            return el;
                        }
                    }
                }

                currentNode = currentNode.Parent;
            }

            return null;
        }


        


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTranslation(long nodeID, string? transl) => _tsltn.SetTranslation(nodeID, transl);


        public bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl)
        {
            if (_tsltn is null)
            {
                transl = null;
                return false;
            }
            else
            {
                return _tsltn.TryGetTranslation(nodeID, out transl);
            }
        }


        public bool GetHasTranslation(long id) => _tsltn.HasTranslation(id);



        public long GetNodeID(XElement node, out string innerXml, out string nodePath)
        {
            lock (_lockObject)
            {
                return _utility.GetNodeID(node, out innerXml, out nodePath);
            }
        }

        public long GetNodeID(XElement node)
        {
            lock (_lockObject)
            {
                return _utility.GetNodeID(node);
            }
        }


        public INode? FindNode(XElement current, string nodePathFragment, bool ignoreCase, bool wholeWord)
        {
            XElement? node = GetNextNode(current);

            while (node != null)
            {
                if (_utility.ContainsPathFragment(_utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node, this);
                }

                node = GetNextNode(node);
            }


            if (_utility.ContainsPathFragment(FirstNode!.NodePath, nodePathFragment, ignoreCase, wholeWord))
            {
                return FirstNode;
            }

            if (object.ReferenceEquals(current, ((Node?)FirstNode)!.XmlNode))
            {
                return null;
            }


            node = GetNextNode(((Node)FirstNode).XmlNode);

            while (node != null && !object.ReferenceEquals(node, current))
            {
                if (_utility.ContainsPathFragment(_utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node, this);
                }

                node = GetNextNode(node);
            }

            return null;
        }

        #endregion


        #region private

        private void InitFirstNode()
        {
            XElement? firstNode = GetFirstNode();

            if (firstNode is null)
            {
                FirstNode = null;
                return;
            }

            FirstNode = new Node(firstNode, this);
        }

        #region private static

        

        private static XElement? ExtractDescendant(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            string name = section.Name.LocalName;
            if (Utility.IsContainerSection(name))
            {
                return Utility.MaskCodeBlock(ExtractDescendantFromContainer(section));
            }
            else if (Utility.IsTranslatable(name))
            {
                return Utility.MaskCodeBlock(section);
            }

            return null;

            /////////////////////////////////////////////////////////////

            static XElement? ExtractDescendantFromContainer(XElement container)
            {
                foreach (XElement innerElement in container.Elements())
                {
                    string innerElementName = innerElement.Name.LocalName;
                    if (Utility.IsContainerSection(innerElementName))
                    {
                        return ExtractDescendantFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElementName))
                    {
                        return innerElement;
                    }
                }

                return null;
            }
        }


        private static XElement? ExtractAncestor(XElement section)
        {
            // Die Reihenfolge ist entscheidend, denn 
            // Utility.IsTranslatable führt nur einen 
            // knappen Negativtest durch!
            if (Utility.IsContainerSection(section.Name.LocalName))
            {
                return Utility.MaskCodeBlock(ExtractAncestorFromContainer(section));
            }
            else if (Utility.IsTranslatable(section.Name.LocalName))
            {
                return Utility.MaskCodeBlock(section);
            }

            return null;

            ///////////////////////////////////////////////////////////////

            static XElement? ExtractAncestorFromContainer(XElement container)
            {
                foreach (XElement innerElement in container.Elements().Reverse())
                {
                    if (Utility.IsContainerSection(innerElement.Name.LocalName))
                    {
                        return ExtractAncestorFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElement.Name.LocalName))
                    {
                        return innerElement;
                    }
                }

                return null;
            }
        }



        #endregion

        #endregion
    }
}
