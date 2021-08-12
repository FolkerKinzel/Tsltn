using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für RemoveUnusedTranslationWindow.xaml
    /// </summary>
    public partial class SelectUnusedTranslationsWindow : Window //, INotifyPropertyChanged
    {
        public SelectUnusedTranslationsWindow(string tsltnFileName, IEnumerable<KeyValuePair<long, string>> unusedTranslations)
        {
            if(unusedTranslations is null)
            {
                throw new ArgumentNullException(nameof(unusedTranslations));
            }

            foreach (KeyValuePair<long, string> item in unusedTranslations)
            {
                var cntr = new UnusedTranslationUserControl(item);
                Controls.Add(cntr);
                cntr._cbSelected.Unchecked += CbSelected_Unchecked;
                cntr._cbSelected.Checked += CbSelected_Checked;
            }

            Controls.Sort((a, b) => StringComparer.Ordinal.Compare(a.Kvp.Value, b.Kvp.Value));

            Explanation = string.Format(CultureInfo.CurrentCulture, Res.UnusedTranslationsExplanation, tsltnFileName);

            Title = string.Format(CultureInfo.CurrentCulture, $"{App.ProgramName} - {Res.UnusedTranslations}");


            InitializeComponent();
        }

        private void CbSelected_Unchecked(object sender, RoutedEventArgs e)
            => _cbAlleKeine.IsChecked = Controls.Any(x => x.Remove) ? null : (bool?)false;


        private void CbSelected_Checked(object sender, RoutedEventArgs e) 
            => _cbAlleKeine.IsChecked = Controls.Any(x => !x.Remove) ? null : (bool?)true;

        //public event PropertyChangedEventHandler? PropertyChanged;

        //private void OnPropertyChanged(string propName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        //}


        public string Explanation { get; }

        public List<UnusedTranslationUserControl> Controls { get; } = new List<UnusedTranslationUserControl>();

        private void OK_Click(object sender, RoutedEventArgs e) => this.DialogResult = true;

        internal bool? ShowDialog(Window owner)
        {
            this.Owner = owner;
            return ShowDialog();
        }

        private void AlleKeine_CheckedChanged(object sender, RoutedEventArgs e)
        {
            bool newVal = (_cbAlleKeine.IsChecked == true);

            foreach (UnusedTranslationUserControl cntr in Controls)
            {
                cntr.Remove = newVal;
            }
        }

        
    }
}
