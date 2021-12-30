using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Tsltn;

/// <summary>
/// Interaktionslogik für SearchUserControl.xaml
/// </summary>
public partial class SearchUserControl : UserControl, INotifyPropertyChanged
{
    public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;
    public event PropertyChangedEventHandler? PropertyChanged;

    private const int MAX_ITEMS = 10;
    private string _pathFragment = "";


    public SearchUserControl() => InitializeComponent();


    public string PathFragment
    {
        get => _pathFragment;
        set
        {
            // Die Überprüfung ist nötig, um eine
            // Endlosschleife zu verhindern:
            if (StringComparer.Ordinal.Equals(_pathFragment, value))
            {
                return;
            }

            value = value?.TrimStart() ?? "";

            _pathFragment = value;
            OnPropertyChanged();

            if (value.Length != 0)
            {
                OnNavigationRequested(value);
            }
        }
    }

    private static List<string> ComboBoxStore { get; } = new List<string>();

    public ObservableCollection<string> ComboBoxItems { get; } = new ObservableCollection<string>();

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        foreach (string item in ComboBoxStore)
        {
            ComboBoxItems.Add(item);
        }

        if (ComboBoxItems.Count != 0)
        {
            PathFragment = ComboBoxItems[0];
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        ComboBoxStore.Clear();
        ComboBoxStore.AddRange(ComboBoxItems);
    }

    public void SetComboBoxItem(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

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
            if (!StringComparer.Ordinal.Equals(ComboBoxItems[0], value))
            {
                ComboBoxItems[0] = value;
            }
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

    private void OnNavigationRequested(string pathFragment) =>
        NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(pathFragment, false, false));

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

}
