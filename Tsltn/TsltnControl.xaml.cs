using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FolkerKinzel.Tsltn.Models;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Tsltn.Resources;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;
using System.Web;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für TsltnControl.xaml
    /// </summary>
    public sealed partial class TsltnControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        private readonly MainWindow _owner;
        private INode _node;
        //private bool _hasDocumentUntranslatedNodes;
        private string _translation = "";
        private readonly IDocument _doc;
        private bool _hasTranslation;
        private string? _sourceLanguage;
        private string? _targetLanguage;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly DataError MissingTranslationWarning = new DataError(ErrorLevel.Warning, Res.UntranslatedElement, null);
        private static readonly DataError InvalidSourceLanguage = new DataError(ErrorLevel.Error, Res.InvalidSourceLanguage, null);
        private static readonly DataError InvalidTargetLanguage = new DataError(ErrorLevel.Error, Res.InvalidTargetLanguage, null);
        private static readonly DataError MissingSourceLanguage = new DataError(ErrorLevel.Information, Res.SourceLanguageNotSpecified, null);
        private static readonly DataError MissingTargetLanguage = new DataError(ErrorLevel.Information, Res.TargetLanguageNotSpecified, null);


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Argumentausnahmen korrekt instanziieren", Justification = "<Ausstehend>")]
        public TsltnControl(MainWindow owner, IDocument doc)
        {
            if (owner is null || doc?.FirstNode is null)
            {
                throw new ArgumentNullException();
            }

            //this.DataContext = this;

            this._owner = owner;
            this._doc = doc;
            this._node = doc.FirstNode!;

            if (_node.Translation != null)
            {
                this._translation = _node.Translation;
                this._hasTranslation = true;
            }
            this.SourceFileName = _doc.SourceDocumentFileName;

            InitializeComponent();

            this.NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;
         
            _owner.TranslationErrors += MainWindow_TranslationErrors;
        }

        
        public bool HasTranslation
        {
            get => _hasTranslation;
            set
            {
                if (_hasTranslation != value)
                {
                    _hasTranslation = value;
                    OnPropertyChanged();

                    if (!value)
                    {
                        Translation = "";
                    }

                    _doc.Tasks.Add(CheckUntranslatedNodesAsync());
                }
            }
        }


        [AllowNull]
        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value ?? "";
                OnPropertyChanged();
            }
        }


        public string? SourceLanguage
        {
            get => _sourceLanguage;
            set
            {
                this.Errors.Remove(MissingSourceLanguage);

                if (string.IsNullOrWhiteSpace(value))
                {
                    _sourceLanguage = null;
                    OnPropertyChanged();
                    this.Errors.Insert(0, MissingSourceLanguage);
                }
                else
                {
                    _sourceLanguage = value.Replace(" ", "", StringComparison.Ordinal);
                    OnPropertyChanged();

                    // wirft ggf. CultureNotFoundException
                    CultureInfo.GetCultureInfoByIetfLanguageTag(_sourceLanguage);
                }
            }
        }


        public string? TargetLanguage
        {
            get => _targetLanguage;
            set
            {
                this.Errors.Remove(MissingTargetLanguage);

                if (string.IsNullOrWhiteSpace(value))
                {
                    _targetLanguage = null;
                    OnPropertyChanged();
                    this.Errors.Insert(0, MissingTargetLanguage);
                }
                else
                {
                    _targetLanguage = value.Replace(" ", "", StringComparison.Ordinal);
                    OnPropertyChanged();

                    // wirft ggf. CultureNotFoundException
                    CultureInfo.GetCultureInfoByIetfLanguageTag(_targetLanguage);
                }
            }
        }


        public string? SourceFileName { get; }

        public INode CurrentNode
        {
            get => _node;

            set
            {
                _node = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<DataError> Errors { get; } = new ObservableCollection<DataError>();


        public void Dispose()
        {
            ((IDisposable)_cancellationTokenSource).Dispose();
        }


        internal void UpdateSource()
        {
            if (HasTranslation)
            {
                _node.Translation = this.Translation.Trim();
            }
            else
            {
                _node.Translation = null;
            }


            if (!Validation.GetHasError(_tbSourceLanguage))
            {
                if (SourceLanguage != _doc.SourceLanguage)
                {
                    _doc.SourceLanguage = SourceLanguage;
                }
            }


            if (!Validation.GetHasError(_tbTargetLanguage))
            {
                if (TargetLanguage != _doc.TargetLanguage)
                {
                    _doc.TargetLanguage = TargetLanguage;
                }
            }
        }

        #region EventHandler

        private async void TsltnControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SourceLanguage = _doc.SourceLanguage;
            this.TargetLanguage = _doc.TargetLanguage;

            await CheckUntranslatedNodesAsync().ConfigureAwait(false);

            _ = Task.Run(() => CheckXmlError(_cancellationTokenSource.Token));
        }


        private void TsltnControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _owner.TranslationErrors -= MainWindow_TranslationErrors;
            _cancellationTokenSource.Cancel();

            this.Dispose();
        }

        private void MainWindow_TranslationErrors(object? sender, TranslationErrorsEventArgs e)
        {
            if (e.Errors.Any())
            {
                this.Errors.Clear();

                foreach (var error in e.Errors)
                {
                    this.Errors.Add(error);
                }

                if (SourceLanguage is null)
                {
                    this.Errors.Add(MissingSourceLanguage);
                }
                else if (Validation.GetHasError(_tbSourceLanguage))
                {
                    this.Errors.Add(InvalidSourceLanguage);
                }

                if (TargetLanguage is null)
                {
                    this.Errors.Add(MissingTargetLanguage);
                }
                else if (Validation.GetHasError(_tbTargetLanguage))
                {
                    this.Errors.Add(InvalidTargetLanguage);
                }

                _doc.Tasks.Add(CheckUntranslatedNodesAsync());
            }
        }


        private void DataError_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is DataError err)
            {
                if (object.ReferenceEquals(err, InvalidSourceLanguage) ||
                    object.ReferenceEquals(err, MissingSourceLanguage))
                {
                    Keyboard.Focus(_tbSourceLanguage);
                }
                else if (object.ReferenceEquals(err, InvalidTargetLanguage) ||
                    object.ReferenceEquals(err, MissingTargetLanguage))
                {
                    Keyboard.Focus(_tbTargetLanguage);
                }
                else
                {
                    Navigate(err.Node);
                    //Keyboard.Focus(control._tbTranslation);
                }
            }
        }


        private void Language_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.OriginalSource is TextBox tb)
            {
                if (tb.Name == nameof(_tbSourceLanguage))
                {
                    this.Errors.Remove(InvalidSourceLanguage);

                    if (Validation.GetHasError(_tbSourceLanguage))
                    {
                        this.Errors.Insert(0, InvalidSourceLanguage);
                    }
                }
                else
                {
                    this.Errors.Remove(InvalidTargetLanguage);

                    if (Validation.GetHasError(_tbTargetLanguage))
                    {
                        this.Errors.Insert(0, InvalidTargetLanguage);
                    }
                }
            }
        }


        private void NavCtrl_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            var target = _node.FindNode(e.PathFragment, !e.CaseSensitive, e.WholeWord);

            if (target is null)
            {
                MessageBox.Show(
                    _owner,
                    string.Format(CultureInfo.CurrentCulture, Res.NoElementFound, e.PathFragment));
            }
            else
            {
                Navigate(target);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _btnReset_Click(object sender, RoutedEventArgs e) => this.HasTranslation = false;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _tbTranslation_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => HasTranslation = true;

        #endregion


        #region Commands

        private void PreviousPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentNode.HasAncestor;
        }

        private void PreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Navigate(_node.GetAncestor());
            e.Handled = true;
        }

        private void NextPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CurrentNode.HasDescendant;
        }

        private void NextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Navigate(CurrentNode.GetDescendant());
            e.Handled = true;
        }

        private void CopyXml_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentNode.InnerXml);
            e.Handled = true;
        }


        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentNode.InnerText);
            e.Handled = true;
        }


        private void BrowseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var allWnd = new BrowseAllTranslationsWindow(_doc.GetAllTranslations());

            if (true == allWnd.ShowDialog(_owner))
            {
                if (allWnd._lbTranslations.SelectedItem is string s)
                {
                    this.HasTranslation = true;
                    this.Translation = s;
                }
            }

            e.Handled = true;
        }

        #endregion


        #region private

        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        private async Task CheckUntranslatedNodesAsync()
        {
            INode? untranslatedNode = await Task.Run(CurrentNode.GetNextUntranslated).ConfigureAwait(true);

            if (untranslatedNode != null)
            {
                MissingTranslationWarning.Node = untranslatedNode;

                if (!Errors.Contains(MissingTranslationWarning))
                {
                    Errors.Insert(0, MissingTranslationWarning);
                }
            }
            else
            {
                Errors.Remove(MissingTranslationWarning);
            }
        }


        private void CheckXmlError(CancellationToken cancelToken)
        {
            while (true)
            {
                Thread.Sleep(5000);

                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }

                if(!_owner.IsCommandEnabled)
                {
                    continue;
                }

                if(!HasTranslation)
                {
                    Dispatcher.Invoke(() => this.RemoveXmlErrorMessages(), DispatcherPriority.SystemIdle);
                    continue;
                }


                if(!_doc.IsValidXml(Translation, out string? exceptionMessage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        RemoveXmlErrorMessages();

                        Errors.Insert(0, new XmlDataError(CurrentNode, exceptionMessage));

                    }, DispatcherPriority.SystemIdle);
                }
                else
                {
                    Dispatcher.Invoke(() => this.RemoveXmlErrorMessages(), DispatcherPriority.SystemIdle);
                }
                
                if (cancelToken.IsCancellationRequested)
                {
                    break;
                }
            }

            Debug.WriteLine("CheckXmlError beendet.");
        }


        private void RemoveXmlErrorMessages()
        {
            var thisErrors = Errors.Where(x => x is XmlDataError && this.CurrentNode.ReferencesSameXml(x.Node!)).ToArray();

            foreach (var error in thisErrors)
            {
                Errors.Remove(error);
            }
        }


        private void Navigate(INode? node)
        {
            if (node is null)
            {
                return;
            }

            UpdateSource();

            //_tbTranslation.IsUndoEnabled = false;
            //_tbTranslation.IsUndoEnabled = true;

            this.CurrentNode = node;

            var transl = node.Translation;
            this.Translation = transl;

            // Die lokale Variable muss benutzt werden,
            // da Translation nie null zurückgibt.
            this.HasTranslation = transl != null;
        }


        #endregion


    }
}
