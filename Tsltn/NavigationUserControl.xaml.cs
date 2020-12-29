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


        public ObservableCollection<string> ComboBoxItems { get; } = new ObservableCollection<string>();

        public bool CaseSensitive { get; set; }

        public bool WholeWord { get; set; } = true;



        private void OnNavigationRequested(string pathFragment, bool caseSensitive, bool wholeWord) =>
            NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(pathFragment, caseSensitive, wholeWord));


        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
             => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void ClearText_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = PathFragment.Length != 0;

        private void ClearText_Executed(object sender, ExecutedRoutedEventArgs e) => PathFragment = "";

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }


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
            if (PathFragment.Length == 0)
            {
                return;
            }

            OnNavigationRequested(PathFragment, CaseSensitive, WholeWord);
            SetComboBoxItem();
        }

        private void SetComboBoxItem()
        {
            // Die lokale Kopie ist nötig, da PathFragment
            // innerhalb der Methode geändert wird.
            string value = PathFragment;
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





    }
}
