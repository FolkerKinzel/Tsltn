using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für <see cref="TsltnPage"/>.xaml
    /// </summary>
    public partial class TsltnPage : Page, INotifyPropertyChanged
    {
        private readonly Window _owner;
        private INode _node;
        private bool _hasDocumentUntranslatedNodes;
        private string? _translation;
        private readonly IDocument _doc;

        public event PropertyChangedEventHandler? PropertyChanged;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Argumentausnahmen korrekt instanziieren", Justification = "<Ausstehend>")]
        public TsltnPage(Window owner, IDocument doc)
        {
            if (owner is null || doc?.FirstNode is null)
            {
                throw new ArgumentNullException();
            }

            this._owner = owner;
            this._doc = doc;
            this._node = doc.FirstNode!;
            this.Translation = _node.Translation;
            this.SourceFileName = System.IO.Path.GetFileName(_doc.SourceDocumentFileName);

            InitializeComponent();

            this.NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;

        }


        private void NavCtrl_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            var target = _node.FindNode(e.PathFragment, e.CaseSensitive);

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



        public string? Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged(nameof(Translation));
            }
        }

        public string? SourceLanguage
        {
            get { return _doc.SourceLanguage; }
            set
            {
                _doc.SourceLanguage = value;
            }
        }

        public string? TargetLanguage
        {
            get { return _doc.TargetLanguage; }
            set
            {
                _doc.TargetLanguage = value;
            }
        }

        public string? SourceFileName { get; }

        public INode? PreviousNode => _node.PreviousNode;

        public INode? NextNode => _node.NextNode;

        public string InnerXml => _node.InnerXml;

        public string NodePath => _node.NodePath;


        internal void GetData()
        {
            _node.Translation = this.Translation;

            this._tbSourceLanguage.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            _doc.SourceLanguage = SourceLanguage;

            this._tbSourceLanguage.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            _doc.TargetLanguage = TargetLanguage;
        }


        private void _btnReset_Click(object sender, RoutedEventArgs e) => this.Translation = null;

        private void PreviousPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = PreviousNode != null;
        }

        private void PreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (PreviousNode != null)
            {
                Navigate(PreviousNode);
            }

            e.Handled = true;
        }

        private void NextPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = NextNode != null;
        }

        private void NextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (NextNode != null)
            {
                Navigate(NextNode);
            }

            e.Handled = true;
        }


        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _hasDocumentUntranslatedNodes;
        }

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            var nextUntranslated = _node?.NextUntranslated;

            if (nextUntranslated is null)
            {
                _hasDocumentUntranslatedNodes = false;
            }
            else
            {
                Navigate(nextUntranslated);
            }
        }

        private void CopyXml_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(InnerXml);
        }


        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(_node.InnerText);
        }



        private void BrowseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }


        private void Navigate(INode node)
        {
            GetData();

            this._node = node;
            this.Translation = node.Translation;

            OnPropertyChanged(nameof(Translation));
            OnPropertyChanged(nameof(InnerXml));
            OnPropertyChanged(nameof(PreviousNode));
            OnPropertyChanged(nameof(NextNode));
            OnPropertyChanged(nameof(NodePath));
        }

        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


    }
}
