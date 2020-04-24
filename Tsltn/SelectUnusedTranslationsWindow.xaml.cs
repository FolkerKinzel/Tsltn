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
using System.Windows.Shapes;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für RemoveUnusedTranslationWindow.xaml
    /// </summary>
    public partial class SelectUnusedTranslationsWindow : Window //, INotifyPropertyChanged
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Argumente von öffentlichen Methoden validieren", Justification = "<Ausstehend>")]
        public SelectUnusedTranslationsWindow(string tsltnFileName, IEnumerable<KeyValuePair<long, string>> unusedTranslations)
        {
            foreach (var item in unusedTranslations)
            {
                Controls.Add(new UnusedTranslationUserControl(item));
            }

            Controls.Sort((a, b) => StringComparer.Ordinal.Compare(a.Kvp.Value, b.Kvp.Value));

            Explanation = string.Format(CultureInfo.CurrentCulture, Res.UnusedTranslationsExplanation, tsltnFileName);

            Title = string.Format(CultureInfo.CurrentCulture, $"{App.PROGRAM_NAME} - {Res.UnusedTranslations}");


            InitializeComponent();
        }

        //public event PropertyChangedEventHandler? PropertyChanged;

        //private void OnPropertyChanged(string propName)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        //}


        public string Explanation { get; }

        public List<UnusedTranslationUserControl> Controls { get; } = new List<UnusedTranslationUserControl>();

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        internal void ShowDialog(Window owner)
        {
            this.Owner = owner;
            ShowDialog();
        }

        private void AlleKeine_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var cnt in Controls)
            {
                cnt.Remove = false;
            }
        }

        private void AlleKeine_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var cnt in Controls)
            {
                cnt.Remove = true;
            }
        }
    }
}
