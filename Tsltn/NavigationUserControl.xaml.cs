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

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für NavigationUserControl.xaml
    /// </summary>
    public partial class NavigationUserControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public NavigationUserControl()
        {
            InitializeComponent();
        }

        private string? _pathFragment;

        public string? PathFragment
        {
            get => _pathFragment;
            
            set
            {
                _pathFragment = value;
                OnPropertyChanged(nameof(PathFragment));
            }
        }

        public bool CaseSensitive { get; set; }

        public bool WholeWord { get; set; } = true;



        private void OnNavigationRequested(string pathFragment, bool caseSensitive, bool wholeWord) =>
            NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(pathFragment, caseSensitive, wholeWord));


        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !string.IsNullOrWhiteSpace(PathFragment);
        }

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DoBrowseForward();
        }

        private void DoBrowseForward()
        {
            PathFragment = PathFragment?.Replace(" ", "", StringComparison.Ordinal);

            if (string.IsNullOrEmpty(PathFragment))
            {
                return;
            }

            OnNavigationRequested(PathFragment, CaseSensitive, WholeWord);
            OnPropertyChanged(nameof(PathFragment));
        }

        

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && !string.IsNullOrWhiteSpace(PathFragment))
            {
                DoBrowseForward();
            }
        }

        private void _btnClear_Click(object sender, RoutedEventArgs e)
        {
            PathFragment = "";
            //CommandManager.InvalidateRequerySuggested();
        }
    }
}
