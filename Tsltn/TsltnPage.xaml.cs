using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private string? _translation;
        private bool _hasDocumentUntranslatedNodes;
        private string? _sourceLanguage;
        private string? _targetLanguage;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TsltnPage(Window owner, INode node, string? sourceLanguage, string? targetLanguage)
        {
            this._owner = owner;
            this._node = node;
            this._translation = node.Translation;
            InitializeComponent();

            this.NavCtrl.Node = _node;
            this.NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;
        }

        private void NavCtrl_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            if (e.Target is null)
            {
                MessageBox.Show(
                    _owner,
                    string.Format(Res.NoElementFound, e.PathFragment));
            }
            else
            {
                Navigate(e.Target);
            }
        }

        private void Navigate(INode node)
        {
            this._node = node;
            this._translation = node.Translation;

            OnPropertyChanged(nameof(Translation));
            OnPropertyChanged(nameof(InnerXml));
            OnPropertyChanged(nameof(PreviousNode));
            OnPropertyChanged(nameof(NextNode));
            OnPropertyChanged(nameof(NodePath));
        }

        private void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));



        public bool Changed { get; set; }


        public string? Translation
        {
            get { return _translation; }
            set { _translation = value; }
        }


        

        public string? SourceLanguage
        {
            get { return _sourceLanguage; }
            set
            { 
                _sourceLanguage = value;
                Changed = true;
            }
        }

        

        public string? TargetLanguage
        {
            get { return _targetLanguage; }
            set 
            {
                _targetLanguage = value;
                Changed = true;
            }
        }




        public INode? PreviousNode => _node.PreviousNode;
        public INode? NextNode => _node.NextNode;

        public string InnerXml => _node.InnerXml;

        public string NodePath => _node.NodePath;


        private void tbTranslation_LostFocus(object sender, RoutedEventArgs e)
        {
            SetTranslation();
            e.Handled = true;
        }

        internal void SetTranslation()
        {
            if (this.Translation != _node.Translation)
            {
                _node.Translation = this.Translation;
                Changed = true;
            }
        }

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

            if(nextUntranslated is null)
            {
                _hasDocumentUntranslatedNodes = false;
            }
            else
            {
                Navigate(nextUntranslated);
            }
        }

        private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
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

        //private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = true;
    }
}
