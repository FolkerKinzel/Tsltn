using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
using FolkerKinzel.RecentFiles.WPF;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public const string PROGRAM_NAME = "Tsltn";

        public const string OnlineHelpUrl = "https://github.com/FolkerKinzel/Tsltn";

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationCommands.SaveAs.InputGestures.Add(
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift, $"{Res.Cntrl}+{Res.ShiftKey}+S"));

            base.OnStartup(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Objekte verwerfen, bevor Bereich verloren geht", Justification = "<Ausstehend>")]
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow(Document.Instance, FileController.GetInstance(Document.Instance, new FileWatcher()),
                new RecentFilesMenu(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!
                ));
            
            MainWindow.Show();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{Res.UnexpectedError}{Environment.NewLine}{Environment.NewLine}{e.Exception.Message}",
                App.PROGRAM_NAME, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            e.Handled = true;
        }

    }
}
