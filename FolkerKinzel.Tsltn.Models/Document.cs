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
    public partial class Document : IDocument, IFileAccess
    {
        private static TsltnFile? _tsltn;
        private static XDocument? _xmlDocument;
        private static readonly object _lockObject = new object();

        /// <summary>
        /// ctor
        /// </summary>
        private Document() { }

        public static Document Instance { get; } = new Document();

        #region IDocument

        #region Properties

        public bool Changed => _tsltn?.Changed ?? false;

        public ConcurrentBag<Task> Tasks { get; } = new ConcurrentBag<Task>();


        public string? TsltnFileName { get; private set; }


        public string? SourceDocumentFileName => _tsltn?.SourceDocumentFileName;


        public string? SourceLanguage
        {
            get => _tsltn?.SourceLanguage;
            set
            {
                if (_tsltn != null)
                {
                    value = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (!StringComparer.Ordinal.Equals(_tsltn.SourceLanguage, value))
                    {
                        _tsltn.SourceLanguage = value;
                    }
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
                    value = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (!StringComparer.Ordinal.Equals(_tsltn.TargetLanguage, value))
                    {
                        _tsltn.TargetLanguage = value;
                    }
                }
            }
        }

        public INode? FirstNode { get; private set; }


        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidXml(string translation, [NotNullWhen(false)] out string? exceptionMessage) => Utility.IsValidXml(translation, out exceptionMessage);


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public async Task WaitAllTasks()
        {
            try
            {
                await Task.WhenAll(Tasks.ToArray()).ConfigureAwait(false);
            }
            catch { }

            Tasks.Clear();
        }


        public void NewTsltn(string sourceDocumentFileName)
        {
            CloseTsltn();

            _xmlDocument = XDocument.Load(sourceDocumentFileName, LoadOptions.None);

            _tsltn = new TsltnFile()
            {
                SourceDocumentFileName = sourceDocumentFileName
            };
            
            InitFirstNode();
        }


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public bool OpenTsltn(string? tsltnFileName)
        {
            CloseTsltn();

            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }

            _tsltn = TsltnFile.Load(tsltnFileName);

            this.TsltnFileName = tsltnFileName;

            return LoadSourceDocument(_tsltn.SourceDocumentFileName);
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



        public void SaveTsltnAs(string tsltnFileName)
        {
            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }

            _tsltn?.Save(tsltnFileName);
            this.TsltnFileName = tsltnFileName;
        }



        public void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations)
        {
            var cloneDoc = new XDocument(_xmlDocument);
            var node = GetFirstNode(cloneDoc);

            errors = new List<DataError>();
            var used = new List<KeyValuePair<long, string>>();

            while (node != null)
            {
                try
                {
                    var trans = GetTranslation(node);

                    if (trans.HasValue)
                    {
                        used.Add(trans.Value);

                        lock (_lockObject)
                        {
                            Utility.Translate(node, trans.Value.Value);
                        }
                    }
                }
                catch (XmlException e)
                {
                    errors.Add(new XmlDataError(new Node(node), e.Message));
                }
                node = GetNextNode(node);
            }

            cloneDoc.Save(outFileName);

            unusedTranslations = GetAllTranslations().Except(used, new KeyValuePairComparer()).ToList();


            /////////////////////////////////////////////

            static KeyValuePair<long, string>? GetTranslation(XElement node)
            {
                long nodePathHash = GetNodeID(node);

                if (TryGetTranslation(nodePathHash, out string? manualTransl))
                {
                    return new KeyValuePair<long, string>(nodePathHash, manualTransl);
                }

                return null;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<long, string>> GetAllTranslations() => _tsltn?.GetAllTranslations() ?? Array.Empty<KeyValuePair<long, string>>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveTranslation(long id) => SetTranslation(id, null);


        public void CloseTsltn()
        {
            _tsltn = null;
            this.TsltnFileName = null;
            FirstNode = null;

            Utility.Cleanup();
        }

        #endregion

        #endregion

        #region internal static

        internal static XElement? GetFirstNode()
        {
            lock (_lockObject)
            {
                return _xmlDocument is null ? null : GetFirstNode(_xmlDocument);

                //var members = _xmlDocument?.Root.Element(Sandcastle.MEMBERS)?.Elements(Sandcastle.MEMBER);

                //if (members is null)
                //{
                //    return null;
                //}

                //foreach (var member in members)
                //{
                //    foreach (XElement section in member.Elements())
                //    {
                //        var el = ExtractDescendant(section);

                //        if (el != null) return el;
                //    }
                //}
            }
        }


        private static XElement? GetFirstNode(XDocument xDoc)
        {
            var members = xDoc.Root.Element(Sandcastle.MEMBERS)?.Elements(Sandcastle.MEMBER);

            if (members is null)
            {
                return null;
            }

            foreach (var member in members)
            {
                foreach (XElement section in member.Elements())
                {
                    var el = ExtractDescendant(section);

                    if (el != null) return el;
                }
            }
        
            return null;
        }

        internal static XElement? GetNextNode(XElement? currentNode)
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
                    foreach (var sibling in currentNode.ElementsAfterSelf())
                    {
                        var el = ExtractDescendant(sibling);

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

                lock (_lockObject)
                {
                    foreach (var sibling in currentNode.ElementsBeforeSelf().Reverse())
                    {
                        var el = ExtractAncestor(sibling);

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
        internal static void SetTranslation(long nodeID, string? transl) => _tsltn?.SetTranslation(nodeID, transl);


        internal static bool TryGetTranslation(long nodeID, [NotNullWhen(true)] out string? transl)
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


        internal static bool GetHasTranslation(long id) => _tsltn?.HasTranslation(id) ?? false;



        internal static long GetNodeID(XElement node, out string innerXml, out string nodePath)
        {
            lock (_lockObject)
            {
                return Utility.GetNodeID(node, out innerXml, out nodePath);
            }
        }

        internal static long GetNodeID(XElement node)
        {
            lock (_lockObject)
            {
                return Utility.GetNodeID(node);
            }
        }


        internal static INode? FindNode(XElement current, string nodePathFragment, bool ignoreCase, bool wholeWord)
        {
            XElement? node = Document.GetNextNode(current);

            while (node != null)
            {
                if (Utility.ContainsPathFragment(Utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
            }


            if (Utility.ContainsPathFragment(Document.Instance.FirstNode!.NodePath, nodePathFragment, ignoreCase, wholeWord))
            {
                return Document.Instance.FirstNode;
            }

            if (object.ReferenceEquals(current, ((Node?)Document.Instance.FirstNode)!.XmlNode))
            {
                return null;
            }


            node = Document.GetNextNode(((Node)Document.Instance.FirstNode).XmlNode);

            while (node != null && !object.ReferenceEquals(node, current))
            {
                if (Utility.ContainsPathFragment(Utility.GetNodePath(node), nodePathFragment, ignoreCase, wholeWord))
                {
                    return new Node(node);
                }

                node = Document.GetNextNode(node);
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

            FirstNode = new Node(firstNode);
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
                foreach (var innerElement in container.Elements())
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
                foreach (var innerElement in container.Elements().Reverse())
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
