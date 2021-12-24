using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models;

public sealed class Document : IDocument, IFileAccess, IDisposable, INotifyPropertyChanged
{
    private readonly TsltnFile _tsltn;
    private readonly FileWatcher? _fileWatcher;

    private string? _fileName;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<ErrorEventArgs>? FileWatcherFailed;
    public event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;
    public event EventHandler<FileSystemEventArgs>? SourceDocumentChanged;


    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="tsltnFile">The <see cref="TsltnFile"/> to work with.</param>
    private Document(TsltnFile tsltnFile)
    {
        _tsltn = tsltnFile;
        Translations = new TranslationsController(_tsltn);
        Navigator = XmlNavigator.Load(tsltnFile.SourceDocumentFileName);
        FirstNode = GetFirstNode();

        if (HasValidSourceDocument)
        {
            _fileWatcher = new FileWatcher(SourceDocumentFileName);
            _fileWatcher.SourceDocumentChanged += FileWatcher_SourceDocumentChanged;
            _fileWatcher.SourceDocumentMoved += FileWatcher_SourceDocumentMoved;
            _fileWatcher.SourceDocumentDeleted += FileWatcher_SourceDocumentDeleted;
            _fileWatcher.FileWatcherError += FileWatcher_FileWatcherError;
        }
    }

    public bool HasSourceDocument => Navigator != null;

    public bool HasValidSourceDocument => FirstNode != null;

    public TranslationsController Translations { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerable<KeyValuePair<long, string>> IDocument.GetAllTranslations()
        => Translations.GetAllTranslations();

    public XmlNavigator? Navigator { get; }

    public INode? FirstNode { get; }

    public bool Changed => _tsltn.Changed;

    public string? FileName
    {
        get => _fileName;
        private set
        {
            _fileName = string.IsNullOrWhiteSpace(value) ? null : value;
            OnPropertyChanged();
        }
    }

    public string? SourceDocumentFileName
    {
        get => _tsltn.SourceDocumentFileName;
        private set
        {
            _tsltn.SourceDocumentFileName = value;
            OnPropertyChanged();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ChangeSourceDocument(string xmlFileName) =>
        SourceDocumentFileName = xmlFileName;

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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                }
            }
        }
    }

    public bool HasError { get; private set; }

    public void Dispose() => _fileWatcher?.Dispose();

    public static Document Create(string xmlFileName)
        => new(new TsltnFile() { SourceDocumentFileName = xmlFileName });


    public static Document Load(string tsltnFileName) 
        => new(TsltnFile.Load(tsltnFileName))
            {
                FileName = tsltnFileName
            };


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveTranslation(long id) => Translations.RemoveTranslation(id);

    public void Save(string tsltnFileName)
    {
        if (tsltnFileName is null)
        {
            throw new ArgumentNullException(nameof(tsltnFileName));
        }

        _tsltn.Save(tsltnFileName);
        FileName = tsltnFileName;
    }

    public (IList<DataError> Errors, IList<KeyValuePair<long, string>> UnusedTranslations) Translate(string outFileName)
    {
        if (Navigator is null || FirstNode is null)
        {
            return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
        }

        var nav = (XmlNavigator)Navigator.Clone();
        XElement? node = nav.GetFirstXElement();

        var used = new List<KeyValuePair<long, string>>();
        var errors = new List<DataError>();

        while (node != null)
        {
            try
            {
                long nodePathHash = nav.GetNodeID(node);
                KeyValuePair<long, string>? trans = Translations.TryGetTranslation(nodePathHash, out string? manualTransl)
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
                errors.Add(new XmlDataError(new Node(node, Translations, nav, new Node(nav.GetFirstXElement()!, Translations, nav, null)), e.Message));
            }
            node = nav.GetNextXElement(node);
        }

        nav.SaveXml(outFileName);

        var unusedTranslations = Translations.GetAllTranslations().Except(used, new KeyValuePairComparer()).ToList();

        return (errors, unusedTranslations);

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

    #region private

    private void OnPropertyChanged([CallerMemberName] string propName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    #region EventHandler
    private void FileWatcher_FileWatcherError(object sender, ErrorEventArgs e)
    {
        if (!HasError)
        {
            HasError = true;
            _fileWatcher?.Dispose();
            FileWatcherFailed?.Invoke(this, e);
        }
    }

    private void FileWatcher_SourceDocumentDeleted(object sender, FileSystemEventArgs e)
    {
        _fileWatcher?.Dispose();
        SourceDocumentFileName = null;
        SourceDocumentDeleted?.Invoke(this, e);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FileWatcher_SourceDocumentMoved(object sender, RenamedEventArgs e)
        => SourceDocumentFileName = e.FullPath;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FileWatcher_SourceDocumentChanged(object sender, FileSystemEventArgs e) =>
        SourceDocumentChanged?.Invoke(this, e);

    #endregion

    private Node? GetFirstNode()
    {
        if (Navigator is null)
        {
            return null;
        }

        XElement? firstXElement = Navigator.GetFirstXElement();
        return firstXElement is null ? null : new Node(firstXElement, Translations, Navigator, null);
    }

    #endregion

}
