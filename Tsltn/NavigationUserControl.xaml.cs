using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private const int MAX_ITEMS = 10;
        private string _pathFragment = "";


        public NavigationUserControl()
        {
            InitializeComponent();
        }


        public string PathFragment
        {
            get => _pathFragment;

            set
            {
                value = value?.Replace(" ", "", StringComparison.Ordinal) ?? "";

                _pathFragment = value;
                OnPropertyChanged();
            }
        }

        private static List<string> ComboBoxStore { get; } = new List<string>();


        public ObservableCollection<string> ComboBoxItems { get; } = new ObservableCollection<string>();

        public bool CaseSensitive { get; set; }

        public bool WholeWord { get; set; } = true;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (string item in ComboBoxStore)
            {
                ComboBoxItems.Add(item);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ComboBoxStore.Clear();
            ComboBoxStore.AddRange(ComboBoxItems);
        }



        private void OnNavigationRequested(string pathFragment, bool caseSensitive, bool wholeWord) =>
            NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(pathFragment, caseSensitive, wholeWord));


        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearText_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = PathFragment.Length != 0;

        private void ClearText_Executed(object sender, ExecutedRoutedEventArgs e) => PathFragment = "";

        //private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //e.Handled = true;
        //    //OnNavigationRequested(PathFragment, CaseSensitive, WholeWord);
        //}

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = PathFragment.Length != 0;

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            OnNavigationRequested(PathFragment, CaseSensitive, WholeWord);
        }

        

        public void SetComboBoxItem(string value)
        {
            int index = ComboBoxItems.IndexOf(value);

            if (index == -1)
            {
                ComboBoxItems.Insert(0, value);
            }
            else if (index > 0)
            {
                ComboBoxItems.RemoveAt(index);
                ComboBoxItems.Insert(0, value);
            }

            _myCb.SelectedIndex = 0;

            if (ComboBoxItems.Count > MAX_ITEMS)
            {
                ComboBoxItems.RemoveAt(ComboBoxItems.Count - 1);
            }
        }

        private void MyCb_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter && PathFragment.Length != 0)
            {
                OnNavigationRequested(PathFragment, CaseSensitive, WholeWord);
            }
        }

        private void ComboBoxItem_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem lbi && lbi.DataContext is string s)
            {
                if (s.Length != 0)
                {
                    OnNavigationRequested(s, CaseSensitive, WholeWord);
                }
            }
        }
    }
}
