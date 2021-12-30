using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Tsltn.Resources;

namespace Tsltn;

/// <summary>
/// Interaktionslogik für BrowseAllTranslationsWindow.xaml
/// </summary>
public partial class BrowseAllTranslationsWindow : Window
{
    public BrowseAllTranslationsWindow(IEnumerable<KeyValuePair<long, string>> enumerable)
    {
        Title = $"{App.ProgramName} - {Res.SelectTranslation}";
        AllTranslations = enumerable.Where(x => !string.IsNullOrWhiteSpace(x.Value))
                                    .Select(x => x.Value)
                                    .Distinct(StringComparer.Ordinal)
                                    .OrderBy(s => s)
                                    .ToArray();
        InitializeComponent();

        _ucSearch.NavigationRequested += UcSearch_NavigationRequested;
    }

    public IEnumerable<string> AllTranslations { get; }

    internal bool TextCopied { get; private set; }

    internal bool? ShowDialog(Window owner)
    {
        Owner = owner;
        return ShowDialog();
    }


    private void UcSearch_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
    {
        string match = AllTranslations.FirstOrDefault(s => s.StartsWith(e.PathFragment, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            _lbTranslations.ScrollIntoView(match);
            _lbTranslations.SelectedItem = match;

            _ucSearch.SetComboBoxItem(e.PathFragment);
        }
    }

    private void OK_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) => DialogResult = true;

    private void MoveUp_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(_lbTranslations.ItemsSource);

        _ = cv.MoveCurrentToPrevious();

        if (cv.IsCurrentBeforeFirst)
        {
            _ = cv.MoveCurrentToFirst();
        }

        _lbTranslations.SelectedItem = cv.CurrentItem;
        _lbTranslations.ScrollIntoView(cv.CurrentItem);
    }

    private void MoveDown_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        ICollectionView cv = CollectionViewSource.GetDefaultView(_lbTranslations.ItemsSource);

        _ = cv.MoveCurrentToNext();

        if (cv.IsCurrentAfterLast)
        {
            _ = cv.MoveCurrentToLast();
        }

        _lbTranslations.SelectedItem = cv.CurrentItem;
        _lbTranslations.ScrollIntoView(cv.CurrentItem);
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            if (FocusManager.GetFocusedElement(this) is not ListBoxItem)
            {
                // Klaut der ComboBox den Focus:
                _ = _btnOK.Focus();
            }

            e.Handled = false;
            return;
        }

        if (Keyboard.FocusedElement is not TextBox) // ?.Name != "PART_EditableTextBox")
        {
            _ = _ucSearch._myCb.Focus();
        }
    }

    private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        Clipboard.SetText(_lbTranslations.SelectedItem as string);
        TextCopied = true;
        _ = Dispatcher.BeginInvoke(new Action(() => DialogResult = true), DispatcherPriority.ApplicationIdle);
    }

}
