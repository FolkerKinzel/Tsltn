﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        private const int MAX_ITEMS = 10;
        private string _pathFragment = "";


        public SearchUserControl()
        {
            InitializeComponent();
        }


        public string PathFragment
        {
            get => _pathFragment;

            set
            {
                value = value?.TrimStart() ?? "";

                // Die Überprüfung ist nötig, um eine
                // Endlosschleife zu verhindern:
                if (value != _pathFragment)
                {
                    _pathFragment = value;

                    SetComboBoxItem(value);
                    OnPropertyChanged();
                }

                if(_pathFragment.Length != 0)
                {
                    OnNavigationRequested();
                }
            }
        }

        private static List<string> ComboBoxStore { get; } = new List<string>();

        public ObservableCollection<string> ComboBoxItems { get; } = new ObservableCollection<string>();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in ComboBoxStore)
            {
                ComboBoxItems.Add(item);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            ComboBoxStore.Clear();
            ComboBoxStore.AddRange(ComboBoxItems);
        }

        private void SetComboBoxItem(string value)
        {
            const StringComparison stringComparison = StringComparison.OrdinalIgnoreCase;

            if (value.Length < 2)
            {
                return;
            }
            string? doublette = ComboBoxItems.FirstOrDefault(s => value.StartsWith(s, stringComparison)); // StringComparer.OrdinalIgnoreCase.Equals(s, value));
            int index = ComboBoxItems.IndexOf(doublette);

            if (index == -1)
            {
                string? longString = ComboBoxItems.FirstOrDefault(s => s.StartsWith(value, stringComparison));
                int indexOfLongString = ComboBoxItems.IndexOf(longString);

                if (indexOfLongString == -1)
                {
                    ComboBoxItems.Insert(0, value);
                }
            }
            else if (index == 0)
            {
                ComboBoxItems[0] = value;
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

        private void ClearText_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = PathFragment.Length != 0;

        private void ClearText_Executed(object sender, ExecutedRoutedEventArgs e) => PathFragment = "";

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }


        private void OnNavigationRequested() =>
            NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(PathFragment, false, false));


        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        
    }
}
