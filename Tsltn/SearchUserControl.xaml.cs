using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für SearchUserControl.xaml
    /// </summary>
    public partial class SearchUserControl : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
        public event PropertyChangedEventHandler? PropertyChanged;

        public SearchUserControl()
        {
            InitializeComponent();
        }

        private string _pathFragment = "";

        public string PathFragment
        {
            get => _pathFragment;

            set
            {
                _pathFragment = value?.TrimStart() ?? "";
                OnPropertyChanged(nameof(PathFragment));

                if(_pathFragment.Length != 0)
                {
                    OnNavigationRequested();
                }
            }
        }




        private void OnNavigationRequested() =>
            NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(PathFragment, false, false));


        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        //private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = !string.IsNullOrWhiteSpace(PathFragment);
        //}

        //private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        //{
        //    e.Handled = true;
        //    DoBrowseForward();
        //}

        //private void DoBrowseForward()
        //{
        //    PathFragment = PathFragment?.Replace(" ", "", StringComparison.Ordinal);

        //    if (string.IsNullOrEmpty(PathFragment))
        //    {
        //        return;
        //    }

        //    OnNavigationRequested();
        //    OnPropertyChanged(nameof(PathFragment));
        //}



        //private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(PathFragment))
        //    {
        //        DoBrowseForward();
        //    }
        //}

        private void _btnClear_Click(object sender, RoutedEventArgs e)
        {
            PathFragment = "";
            //CommandManager.InvalidateRequerySuggested();
        }
    }
}
