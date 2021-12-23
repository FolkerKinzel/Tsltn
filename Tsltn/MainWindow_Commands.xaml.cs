using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using FolkerKinzel.RecentFiles.WPF;
using FolkerKinzel.Tsltn.Controllers;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using Tsltn.Resources;

namespace Tsltn
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow
    {
        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.CurrentDocument != null;
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = Controller.CurrentDocument?.Changed ?? false;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Help_Executed(object sender, ExecutedRoutedEventArgs e) => new HelpWindow().Show();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void New_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = NewDocumentAsync();
        

        private void Open_ExecutedAsync(object sender, ExecutedRoutedEventArgs e)
        {
            if (GetTsltnInFileName(out string tsltnFileName))
            {
                _ = OpenDocumentAsync(tsltnFileName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async void Close_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => await CloseCurrentDocumentAsync();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Save_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = SaveCurrentDocumentAsync(false);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SaveAs_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = SaveCurrentDocumentAsync(true);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Translate_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = TranslateCurrentDocumentAsync();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeSourceDocument_ExecutedAsync(object sender, ExecutedRoutedEventArgs e) => _ = ChangeSourceDocumentAsync();


    }
}
