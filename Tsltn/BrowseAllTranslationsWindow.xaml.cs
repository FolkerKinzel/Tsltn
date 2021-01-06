using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für BrowseAllTranslationsWindow.xaml
    /// </summary>
    public partial class BrowseAllTranslationsWindow : Window
    {

        public BrowseAllTranslationsWindow(IEnumerable<KeyValuePair<long, string>> enumerable)
        {
            this.Title = $"{App.PROGRAM_NAME} - {Res.SelectTranslation}";
            this.AllTranslations = enumerable.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => x.Value).Distinct(StringComparer.Ordinal).OrderBy(s => s).ToArray();

            InitializeComponent();

            _ucSearch.NavigationRequested += UcSearch_NavigationRequested;
        }

        public IEnumerable<string> AllTranslations { get; }


        private void UcSearch_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            string match = AllTranslations.FirstOrDefault(s => s.StartsWith(e.PathFragment, StringComparison.OrdinalIgnoreCase));

            if(match != null)
            {
                _lbTranslations.ScrollIntoView(match);
                _lbTranslations.SelectedItem = match;

                _ucSearch.SetComboBoxItem(e.PathFragment);
            }
        }

        internal bool? ShowDialog(Window owner)
        {
            this.Owner = owner;
            return ShowDialog();
        }


        private void OK_Click(object sender, RoutedEventArgs e) => this.DialogResult = true;


        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) => this.DialogResult = true;

        private void MoveUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ICollectionView cv = CollectionViewSource.GetDefaultView(_lbTranslations.ItemsSource);

            cv.MoveCurrentToPrevious();

            if (cv.IsCurrentBeforeFirst)
            {
                cv.MoveCurrentToFirst();
            }


            _lbTranslations.SelectedItem = cv.CurrentItem;
            _lbTranslations.ScrollIntoView(cv.CurrentItem);
        }

        private void MoveDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ICollectionView cv = CollectionViewSource.GetDefaultView(_lbTranslations.ItemsSource);

            cv.MoveCurrentToNext();

            if (cv.IsCurrentAfterLast)
            {
                cv.MoveCurrentToLast();
            }

            _lbTranslations.SelectedItem = cv.CurrentItem;
            _lbTranslations.ScrollIntoView(cv.CurrentItem);
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if(!(FocusManager.GetFocusedElement(this) is ListBoxItem))
                {
                    // Klaut der ComboBox den Focus:
                    _btnOK.Focus();
                }
                
                e.Handled = false;

                return;
            }

            if (!(Keyboard.FocusedElement is TextBox)) // ?.Name != "PART_EditableTextBox")
            {
                _ucSearch._myCb.Focus();
            }
        }


        //private void CopyText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        //    => e.CanExecute = !string.IsNullOrWhiteSpace(_lbTranslations?.SelectedItem as string);


        private void CopyText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetText(_lbTranslations.SelectedItem as string);
            Dispatcher.BeginInvoke( new Action(() => this.DialogResult = false), DispatcherPriority.ApplicationIdle);
        }

        
    }
}
