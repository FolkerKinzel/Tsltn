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

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für TsltnControl.xaml
    /// </summary>
    public partial class TsltnControl : UserControl, INotifyPropertyChanged
    {
        private readonly Window _owner;
        private INode _node;
        //private bool _hasDocumentUntranslatedNodes;
        private string _translation = "";
        private readonly IDocument _doc;
        private bool _hasTranslation;
        private string? _sourceLanguage;
        private string? _targetLanguage;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<ValidationErrorEventArgs>? LanguageErrorChanged;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Argumentausnahmen korrekt instanziieren", Justification = "<Ausstehend>")]
        public TsltnControl(Window owner, IDocument doc)
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
            this.SourceFileName = System.IO.Path.GetFileName(_doc.SourceDocumentFileName);
            
            InitializeComponent();

            this.NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;
        }


        public bool HasTranslation
        {
            get => _hasTranslation;
            set
            {
                _hasTranslation = value;
                OnPropertyChanged();

                if (!value)
                {
                    Translation = "";
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
                if (string.IsNullOrWhiteSpace(value))
                {
                    _sourceLanguage = null;
                    OnPropertyChanged();
                }
                else
                {
                    _sourceLanguage = value.Replace(" ","", StringComparison.Ordinal);
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
                if (string.IsNullOrWhiteSpace(value))
                {
                    _targetLanguage = null;
                    OnPropertyChanged();
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


        internal void Navigate(INode? node)
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


        internal void UpdateSource()
        {
            if (HasTranslation)
            {
                this._tbTranslation.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                _node.Translation = this.Translation;
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

        private void TsltnControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.SourceLanguage = _doc.SourceLanguage;
            this.TargetLanguage = _doc.TargetLanguage;
        }


        private void NavCtrl_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            var target = _node.FindNode(e.PathFragment, e.CaseSensitive, e.WholeWord);

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
                if (allWnd._lbTranslations.SelectedItem is KeyValuePair<long, string> kvp)
                {
                    this.HasTranslation = true;
                    this.Translation = kvp.Value;
                }
            }

            e.Handled = true;
        }

        #endregion


        #region private

        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnLanguageErrorChanged(ValidationErrorEventArgs e) => LanguageErrorChanged?.Invoke(this, e);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Language_Error(object sender, ValidationErrorEventArgs e) => OnLanguageErrorChanged(e);

        #endregion
    }
}
