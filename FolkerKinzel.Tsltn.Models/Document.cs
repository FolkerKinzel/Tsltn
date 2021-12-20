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
    public partial class Document : IDocument, IFileAccess, ITranslation
    {
        private readonly TsltnFile _tsltn;

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

            Navigator = XmlNavigator.Load(tsltnFile.SourceDocumentFileName);
            FirstNode = GetFirstNode();
        }

        public XmlNavigator? Navigator { get; }

        public INode? FirstNode { get; }


        private void Tsltn_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TsltnFile.Changed))
            {
                OnPropertyChanged(nameof(Changed));
            }
        }



        #region Methods

        public static Document Create(string sourceDocumentFileName)
            => new Document(new TsltnFile() { SourceDocumentFileName = sourceDocumentFileName });



        public static Document Load(string tsltnFileName)
        {
            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }

            return new Document(TsltnFile.Load(tsltnFileName));
        }


        //public bool ReloadSourceDocument(string fileName)
        //{
        //    Debug.Assert(_tsltn != null);

        //    if(LoadSourceDocument(fileName))
        //    {
        //        _tsltn.SourceDocumentFileName = fileName;
        //        return true;
        //    }

        //    return false;
        //}



        public void Save(string tsltnFileName)
        {
            if (tsltnFileName is null)
            {
                throw new ArgumentNullException(nameof(tsltnFileName));
            }

            _tsltn.Save(tsltnFileName);
            //this.TsltnFileName = tsltnFileName;
        }



        public void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations)
        {
            errors = new List<DataError>();

            if (Navigator is null || FirstNode is null)
            {
                unusedTranslations = new List<KeyValuePair<long, string>>();
                return;
            }

            var nav = (XmlNavigator)Navigator.Clone();
            XElement? node = nav.GetFirstXElement();

            var used = new List<KeyValuePair<long, string>>();

            while (node != null)
            {
                try
                {
                    long nodePathHash = nav.GetNodeID(node);
                    KeyValuePair<long, string>? trans = TryGetTranslation(nodePathHash, out string? manualTransl)
                                                            ? new KeyValuePair<long, string>(nodePathHash, manualTransl)
                                                            : (KeyValuePair<long, string>?)null;

                    if (trans.HasValue)
                    {
                        used.Add(trans.Value);
                        Translate(node, trans.Value.Value);
                    }
                }
                catch (XmlException e)
                {
                    errors.Add(new XmlDataError(new Node(node, this, nav, new Node(nav.GetFirstXElement()!, this, nav, null)), e.Message));
                }
                node = nav.GetNextXElement(node);
            }

            nav.SaveXml(outFileName);

            unusedTranslations = GetAllTranslations().Except(used, new KeyValuePairComparer()).ToList();

            ///////////////////////////////////////

            /// <summary>
            /// Ersetzt das innere Xml von <paramref name="node"/> durch das in 
            /// <paramref name="translation"/> enthaltene Xml.
            /// </summary>
            /// <param name="node">Das <see cref="XElement"/>, das übersetzt wird.</param>
            /// <param name="translation">XML, das den übersetzten Inhalt von <paramref name="node"/>
            /// bilden soll.</param>
            /// <exception cref="XmlException">Der Inhalt von <paramref name="translation"/> war kein gültiges Xml.</exception>
            static void Translate(XElement node, string translation)
            {
                var tmp = XElement.Parse($"<R>{translation}</R>", LoadOptions.None);

                node.ReplaceNodes(tmp.Nodes());

                if (node is XCodeCloneElement clone)
                {
                    XElement[] sourceCodeNodes = clone.Source.Elements(Sandcastle.CODE).ToArray();
                    XElement[] nodeCodeNodes = node.Elements(Sandcastle.CODE).ToArray();

                    int end = Math.Min(sourceCodeNodes.Length, nodeCodeNodes.Length);

                    for (int i = 0; i < end; i++)
                    {
                        nodeCodeNodes[i].ReplaceWith(sourceCodeNodes[i]);
                    }

                    clone.Source.ReplaceNodes(clone.Nodes());
                }
            }
        }

        #endregion


        #region private

        private Node? GetFirstNode()
        {
            if(Navigator is null)
            {
                return null;
            }

            XElement? firstXElement = Navigator.GetFirstXElement();
            return firstXElement is null ? null : new Node(firstXElement, this, Navigator, null);
        }

        #endregion



    }
}
