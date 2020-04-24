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


        public event PropertyChangedEventHandler? PropertyChanged;



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
            this.SourceLanguage = _doc.SourceLanguage;
            this.TargetLanguage = _doc.TargetLanguage;

            InitializeComponent();

            this.NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;
        }

        public bool HasTranslation
        {
            get => _hasTranslation;
            set
            {
                _hasTranslation = value;
                OnPropertyChanged(nameof(HasTranslation));

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
                OnPropertyChanged(nameof(Translation));
            }
        }

        public string? SourceLanguage { get; set; }


        public string? TargetLanguage { get; set; }


        public string? SourceFileName { get; }

        //public bool HasNodeAncestor => _node.HasAncestor;

        //public bool HasNodeDescendant => _node.HasDescendant;

        //public string InnerXml => _node.InnerXml;

        //public string NodePath => _node.NodePath;


        public INode CurrentNode
        {
            get => _node;

            set
            {
                _node = value;
                OnPropertyChanged(nameof(CurrentNode));
            }
        }


        internal void Navigate(INode? node)
        {
            if (node is null)
            {
                return;
            }

            UpdateSource();

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

            this._tbSourceLanguage.GetBindingExpression(TextBox.TextProperty).UpdateSource();


            SourceLanguage = string.IsNullOrWhiteSpace(SourceLanguage) ? null : SourceLanguage.Replace(" ", "", StringComparison.Ordinal);


            if(SourceLanguage != _doc.SourceLanguage)
            {
                _doc.SourceLanguage = SourceLanguage;
            }


            this._tbTargetLanguage.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            TargetLanguage = string.IsNullOrWhiteSpace(TargetLanguage) ? null : TargetLanguage.Replace(" ", "", StringComparison.Ordinal);

            if (TargetLanguage != _doc.TargetLanguage)
            {
                _doc.TargetLanguage = TargetLanguage;
            }
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

        private void _btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.HasTranslation = false;
        }

        private void _tbTranslation_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            HasTranslation = true;
        }

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


        //private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = _hasDocumentUntranslatedNodes;
        //}

        //private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    e.Handled = true;

        //    var nextUntranslated = _node?.GetNextUntranslated();

        //    if (nextUntranslated is null)
        //    {
        //        _hasDocumentUntranslatedNodes = false;
        //    }
        //    else
        //    {
        //        Navigate(nextUntranslated);
        //    }
        //}

        private void CopyXml_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentNode.InnerXml);
        }


        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentNode.InnerText);
        }



        private void BrowseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var allWnd = new BrowseAllTranslationsWindow(_doc.GetAllTranslations());

            allWnd.ShowDialog(_owner);

            if(allWnd._lbTranslations.SelectedItem is KeyValuePair<long, string> kvp)
            {
                this.HasTranslation = true;
                this.Translation = kvp.Value;
            }
        }


        

    

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        
    }
}
