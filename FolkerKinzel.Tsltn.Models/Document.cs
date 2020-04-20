using FolkerKinzel.Tsltn.Models.Intls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace FolkerKinzel.Tsltn.Models
{
    public class Document : IDocument
    {
        private static TsltnFile? _tsltn;
        private static XDocument? _xmlDocument;

        /// <summary>
        /// ctor
        /// </summary>
        private Document() { }

        public static Document Instance { get; } = new Document();

        #region IDocument

        #region Properties

        public bool Changed => _tsltn?.Changed ?? false;

     

        public string? TsltnFileName { get; private set; }


        public string SourceDocumentFileName
        {
            get => _tsltn?.SourceDocumentFileName ?? "";
            //set
            //{
            //    if (_tsltn != null)
            //    {
            //        _tsltn.SourceDocumentFileName = value;
            //    }
            //}
        }

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

        public bool SourceDocumentExists { get; private set; }

        #endregion

        #region Methods

        public void NewTsltn(string sourceDocumentFileName)
        {
            if (Changed)
            {
                throw new InvalidOperationException();
            }

            Close();

            _xmlDocument = XDocument.Load(sourceDocumentFileName, LoadOptions.None);

            _tsltn = new TsltnFile(sourceDocumentFileName);
            

            InitFirstNode();
        }


        public void Open(string? tsltnFileName)
        {
            if (Changed)
            {
                throw new InvalidOperationException();
            }

            Close();

            if (tsltnFileName is null)
            {
                return;
            }

            _tsltn = TsltnFile.Load(tsltnFileName);

            this.TsltnFileName = tsltnFileName;

            


            string xmlFileName = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(tsltnFileName), _tsltn.SourceDocumentFileName));

            if (!File.Exists(xmlFileName))
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
            out List<(XmlException Exception, INode Node)> errors,
            out List<KeyValuePair<long, string>> unused)
        {
            var node = ((Node?)FirstNode)?.XmlNode;

            errors = new List<(XmlException Exception, INode Node)>();

            var used = new List<KeyValuePair<long, string>>();

            while (node != null)
            {
                try
                {
                    var trans = GetTranslation(node);

                    if (trans.HasValue)
                    {
                        used.Add(trans.Value);
                        Utility.Translate(node, trans.Value.Value);
                    }
                }
                catch (XmlException e)
                {
                    errors.Add((e, new Node(node)));
                }
                node = GetNextNode(node);
            }

            _xmlDocument?.Save(outFileName);

            this.Open(this.TsltnFileName);

            unused = GetAllTranslations().Except(used, new KeyValuePairComparer()).ToList();

            /////////////////////////////////////////////

            static KeyValuePair<long, string>? GetTranslation(XElement node)
            {
                long nodePathHash = Utility.GetNodeID(node);

                if (TryGetTranslation(nodePathHash, out string? manualTransl))
                {
                    return new KeyValuePair<long, string>(nodePathHash, manualTransl);
                }

                return null;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public IEnumerable<KeyValuePair<long, string>> GetAllTranslations() => _tsltn?.GetAllTranslations() ?? Array.Empty<KeyValuePair<long, string>>();


        [SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public void RemoveUnusedTranslations(IEnumerable<KeyValuePair<long, string>> unused)
        {
            if (unused is null)
            {
                throw new ArgumentNullException(nameof(unused));
            }

            foreach (var kvp in unused)
            {
                SetTranslation(kvp.Key, null);
            }
        }


        public void Close()
        {
            _tsltn = null;
            this.TsltnFileName = null;
            this.SourceDocumentExists = false;
            FirstNode = null;
            //Node.ClearNodeContainer();
        }

        #endregion

        #endregion

        #region internal static

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

                foreach (var sibling in currentNode.ElementsAfterSelf())
                {
                    var el = ExtractDescendant(sibling);

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
                    var el = ExtractAncestor(sibling);

                    if (el != null)
                    {
                        return el;
                    }
                }

                currentNode = currentNode.Parent;
            }

            return null;
        }


        internal static XElement? GetNextUntranslated(XElement node)
        {
            if(_tsltn is null)
            {
                return null;
            }

            XElement? unTrans = GetNextNode(node);
            while (unTrans != null)
            {
                if (_tsltn.HasTranslation(Utility.GetNodeID(node)))
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
            }

            unTrans = GetFirstNode();

            while (unTrans != null && !object.ReferenceEquals(unTrans, node))
            {
                if (_tsltn.HasTranslation(Utility.GetNodeID(unTrans)))
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
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
            if (Utility.IsContainerSection(section))
            {
                return Utility.MaskCodeBlock(ExtractDescendantFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.MaskCodeBlock(section);
            }

            return null;

            /////////////////////////////////////////////////////////////

            static XElement? ExtractDescendantFromContainer(XElement container)
            {
                foreach (var innerElement in container.Elements())
                {
                    if (Utility.IsContainerSection(innerElement))
                    {
                        return ExtractDescendantFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElement))
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
            if (Utility.IsContainerSection(section))
            {
                return Utility.MaskCodeBlock(ExtractAncestorFromContainer(section));
            }
            else if (Utility.IsTranslatable(section))
            {
                return Utility.MaskCodeBlock(section);
            }

            return null;

            ///////////////////////////////////////////////////////////////

            static XElement? ExtractAncestorFromContainer(XElement container)
            {
                foreach (var innerElement in container.Elements().Reverse())
                {
                    if (Utility.IsContainerSection(innerElement))
                    {
                        return ExtractAncestorFromContainer(innerElement);
                    }
                    else if (Utility.IsTranslatable(innerElement))
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
