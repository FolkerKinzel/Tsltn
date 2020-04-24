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
using System.Windows.Shapes;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für BrowseAllTranslationsWindow.xaml
    /// </summary>
    public partial class BrowseAllTranslationsWindow : Window //, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;


        public BrowseAllTranslationsWindow(IEnumerable<KeyValuePair<long, string>> enumerable)
        {
            this.Title = $"{App.PROGRAM_NAME} - {Res.SelectTranslation}";
            this.AllTranslations = enumerable;
            InitializeComponent();
        }

        public IEnumerable<KeyValuePair<long, string>> AllTranslations { get; }


        //private void OnPropertyChanged(string propName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        //}

        internal bool? ShowDialog(Window owner)
        {
            this.Owner = owner;
            return ShowDialog();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
