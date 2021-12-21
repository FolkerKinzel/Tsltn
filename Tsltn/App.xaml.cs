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
using System.Text.RegularExpressions;

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application
    {
        public const string ProgramName = "Tsltn";

        public const string OnlineHelpUrl = "https://github.com/FolkerKinzel/Tsltn";

        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationCommands.SaveAs.InputGestures.Add(
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift, $"{Res.Cntrl}+{Res.ShiftKey}+S"));

            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow(FileController.Instance,
                new RecentFilesMenu(
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)!
                ));
            
            MainWindow.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"{Res.UnexpectedError}{Environment.NewLine}{Environment.NewLine}{e.Exception.Message}",
                App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            e.Handled = true;
        }

    }
}
