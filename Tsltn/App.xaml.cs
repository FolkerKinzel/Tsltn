﻿using FolkerKinzel.Tsltn.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string PROGRAM_NAME = "Tsltn";
        public const string TSLTN_FILE_EXTENSION = ".tsltn";

        //private void OnStartup(object sender, StartupEventArgs e)
        //{
        //    ApplicationCommands.SaveAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
        //}


        protected override void OnStartup(StartupEventArgs e)
        {
            ApplicationCommands.SaveAs.InputGestures.Add(
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift, $"{Res.Cntrl}+{Res.Shift}+S"));

            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new MainWindow(Document.Instance, new RecentFilesMenu()).Show();
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
