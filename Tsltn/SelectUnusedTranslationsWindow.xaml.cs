using System.Globalization;
using System.Windows;
using Tsltn.Resources;

namespace Tsltn;

/// <summary>
/// Interaktionslogik für RemoveUnusedTranslationWindow.xaml
/// </summary>
public partial class SelectUnusedTranslationsWindow : Window //, INotifyPropertyChanged
{
    public SelectUnusedTranslationsWindow(string? tsltnFileName, IEnumerable<KeyValuePair<long, string>> unusedTranslations)
    {
        if (unusedTranslations is null)
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


    public string Explanation { get; }

    public List<UnusedTranslationUserControl> Controls { get; } = new List<UnusedTranslationUserControl>();

    private void OK_Click(object sender, RoutedEventArgs e) => DialogResult = true;

    internal bool? ShowDialog(Window owner)
    {
        Owner = owner;
        return ShowDialog();
    }

    private void AlleKeine_CheckedChanged(object sender, RoutedEventArgs e)
    {
        bool newVal = _cbAlleKeine.IsChecked == true;

        foreach (UnusedTranslationUserControl cntr in Controls)
        {
            cntr.Remove = newVal;
        }
    }


}
