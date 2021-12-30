using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Controllers;
using Tsltn.Resources;

namespace Tsltn;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public sealed partial class App : Application
{
    public const string ProgramName = "Tsltn";
    public const string TsltnFileExtension = ".tsltn";
    public const string OnlineHelpUrl = "https://github.com/FolkerKinzel/Tsltn";

    protected override void OnStartup(StartupEventArgs e)
    {
        Environment.CurrentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _ = ApplicationCommands.SaveAs.InputGestures.Add(
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
        _ = MessageBox.Show($"{Res.UnexpectedError}{Environment.NewLine}{Environment.NewLine}{e.Exception.Message}",
            App.ProgramName, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

        e.Handled = true;
    }

}
