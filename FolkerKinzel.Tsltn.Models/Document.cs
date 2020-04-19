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
        
            _tsltn = new TsltnFile
            {
                SourceDocumentFileName = sourceDocumentFileName
            };

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

            _tsltn?.Save(TsltnFileName);
            this.TsltnFileName = tsltnFileName;
        }



        //public void SaveTsltn()
        //{
        //    SaveTsltnAs(this.TsltnFileName!);
        //}


        public void Translate(
            string outFileName,
            out List<(XmlException Exception, INode Node)> errors,
            out List<(bool IsManualTranslation, int Hash, string Text)> unused)
        {

            //this.SaveTsltnAs(TsltnFileName);

            var node = ((Node?)FirstNode)?.XmlNode;

            errors = new List<(XmlException Exception, INode Node)>();

            var used = new List<(bool IsManualTranslation, int Hash, string Text)>();

            while (node != null)
            {
                try
                {
                    var trans = GetTranslation(node);

                    if (trans.HasValue)
                    {
                        used.Add(trans.Value);
                        Utility.Translate(node, trans.Value.Text);
                    }
                }
                catch (XmlException e)
                {
                    errors.Add((e, Node.GetNode(node)));
                }
                node = GetNextNode(node);
            }

            _xmlDocument?.Save(outFileName);

            this.Open(this.TsltnFileName);



            unused = GetAllTranslations().Except(used, new TranslationTupleComparer()).ToList();


            /////////////////////////////////////////////

            static (bool IsManualTranslation, int Hash, string Text)? GetTranslation(XElement node)
            {
                int nodePathHash = Utility.GetNodePathHash(node);

                if (TryGetManualTranslation(nodePathHash, out string? manualTransl))
                {
                    return (true, nodePathHash, manualTransl);
                }

                int contentHash = Utility.GetContentHash(node, out string _);

                string? autoTransl = GetAutoTranslation(contentHash);

                return autoTransl is null ? ((bool IsManualTranslation, int Hash, string Text)?)null : (IsManualTranslation: false, Hash: contentHash, Text: autoTransl);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public List<(bool IsManualTranslation, int Hash, string Text)> GetAllTranslations() => _tsltn?.GetAllTranslations() ?? new List<(bool IsManualTranslation, int Hash, string Text)>();


        [SuppressMessage("Performance", "CA1822:Member als statisch markieren", Justification = "<Ausstehend>")]
        public void RemoveUnusedTranslations(IEnumerable<(bool IsManualTranslation, int Hash, string Text)> unused)
        {
            if (unused is null)
            {
                throw new ArgumentNullException(nameof(unused));
            }

            foreach (var (IsManualTranslation, Hash, _) in unused)
            {
                if (IsManualTranslation)
                {
                    SetManualTranslation(Hash, null);
                }
                else
                {
                    SetAutoTranslation(Hash, null);
                }
            }
        }


        public void Close()
        {
            _tsltn = null;
            this.TsltnFileName = null;
            this.SourceDocumentExists = false;
            FirstNode = null;
            Node.ClearNodeContainer();
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
            XElement? unTrans = GetNextNode(node);
            while (unTrans != null)
            {
                if (HasTranslation(node))
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
            }

            unTrans = GetFirstNode();

            while (unTrans != null && !object.ReferenceEquals(unTrans, node))
            {
                if (HasTranslation(unTrans))
                {
                    return unTrans;
                }

                unTrans = GetNextNode(unTrans);
            }

            return null;

            /////////////////////////////////////////

            static bool HasTranslation(XElement node) =>
                HasManualTranslation(Utility.GetNodePathHash(node)) || HasAutoTranslation(Utility.GetContentHash(node, out string _));
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetAutoTranslation(int contentHash, string? transl) => _tsltn?.SetAutoTranslation(contentHash, transl);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetManualTranslation(int nodePathHash, string? transl) => _tsltn?.SetManualTranslation(nodePathHash, transl);


        internal static bool TryGetManualTranslation(int nodePathHash, [NotNullWhen(true)] out string? transl)
        {
            if (_tsltn is null)
            {
                transl = null;
                return false;
            }
            else
            {
                return _tsltn.TryGetManualTranslation(nodePathHash, out transl);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasManualTranslation(int nodePathHash) => _tsltn?.HasManualTranslation(nodePathHash) ?? false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool HasAutoTranslation(int contentHash) => _tsltn?.HasAutoTranslation(contentHash) ?? false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string? GetAutoTranslation(int contentHash) => _tsltn?.GetAutoTranslation(contentHash);


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
