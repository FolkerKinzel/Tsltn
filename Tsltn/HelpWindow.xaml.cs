using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            Title = $"{App.ProgramName} - {Res.HelpWindowTitle}";
            InitializeComponent();
            _fdsvContent.Document = (FlowDocument)XamlReader.Parse(Res.Help);
        }

        private void OK_Click(object sender, RoutedEventArgs e) => Close();
    }
}
