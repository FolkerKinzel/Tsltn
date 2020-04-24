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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für UnusedTranslationUserControl.xaml
    /// </summary>
    public partial class UnusedTranslationUserControl : UserControl, INotifyPropertyChanged
    {
        private bool _remove;

        //public UnusedTranslationUserControl() { }

        public UnusedTranslationUserControl(KeyValuePair<long, string> item)
        {
            Kvp = item;
            InitializeComponent();
        }


        public KeyValuePair<long, string> Kvp { get; }

        public bool Remove
        {
            get => _remove;
            
            set
            {
                _remove = value;
                OnPropertyChanged(nameof(Remove));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
