using System.ComponentModel;
using System.Windows.Controls;

namespace Tsltn;

/// <summary>
/// Interaktionslogik für UnusedTranslationUserControl.xaml
/// </summary>
public partial class UnusedTranslationUserControl : UserControl, INotifyPropertyChanged
{
    private bool _remove;

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
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
