using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using FolkerKinzel.Tsltn.Models;
using Tsltn.Resources;

namespace Tsltn;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public sealed partial class MainWindow
{
    private void RefreshData()
    {
        if (_ccContent.Content is TsltnControl control)
        {
            control.UpdateSource();
        }
    }

    private async void ShowCurrentDocument()
    {
        IDocument? doc = Controller.CurrentDocument;
        if (doc is null)
        {
            _ccContent.Content = null;
            return;
        }

        var cntr = new TsltnControl(this, doc);
        _ccContent.Content = cntr;
        _ = cntr._tbOriginal.Focus();

        string? fileName = doc.FileName;

        if (fileName != null)
        {
            _tasks.Add(_recentFilesMenu.AddRecentFileAsync(fileName));
        }

        if (doc.HasValidSourceDocument)
        {
            doc.PropertyChanged += Doc_PropertyChanged;
            doc.FileWatcherFailed += Doc_FileWatcherFailed;
            doc.SourceDocumentDeleted += Doc_SourceDocumentDeleted;
            doc.SourceDocumentChanged += Doc_SourceDocumentChanged;
        }
        else
        {
            string errorMessage = doc.HasSourceDocument
                                    ? string.Format(
                                      CultureInfo.CurrentCulture,
                                      Res.EmptyOrInvalidFile,
                                      Environment.NewLine, System.IO.Path.GetFileName(doc.SourceDocumentFileName), Res.XmlDocumentationFile)
                                    : string.Format(CultureInfo.CurrentCulture, Res.SourceDocumentNotFound, Environment.NewLine, doc.SourceDocumentFileName);

            ShowMessage(errorMessage, MessageBoxImage.Error);


            if (!await ChangeSourceDocumentAsync().ConfigureAwait(false))
            {
                Controller.CloseCurrentDocument();
            }
        }
    }


    private void ShowMessage(string message, MessageBoxImage icon) =>
        _ = MessageBox.Show(this, message, App.ProgramName, MessageBoxButton.OK,
                            icon, MessageBoxResult.OK);


    private async Task NewDocumentAsync()
    {
        string? xmlFileName = null;
        if (GetXmlInFileName(ref xmlFileName))
        {
            if (!await CloseCurrentDocumentAsync().ConfigureAwait(true))
            {
                return;
            }

            try
            {
                await Task.Run(() => Controller.NewDocument(xmlFileName)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, Res.CreationFailed, Environment.NewLine, ex.Message);
                _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
            }
        }
    }


    private async Task OpenDocumentAsync(string tsltnFileName)
    {
        if (!await CloseCurrentDocumentAsync().ConfigureAwait(true))
        {
            return;
        }

        try
        {
            await Task.Run(() => Controller.OpenTsltnDocument(tsltnFileName)).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            string errorMessage = string.Format(CultureInfo.InvariantCulture, Res.OpenFileFailed, Environment.NewLine, tsltnFileName, ex.Message);
            _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
        }
    }


    private async Task ProcessCommandLineArgs()
    {
        string[] args = Environment.GetCommandLineArgs();

        if (args.Length > 1)
        {
            try
            {
                string fileName = Path.GetFullPath(args[1]);
                await Task.Run(() => Controller.OpenTsltnDocument(fileName)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, Res.OpenFileFailed, Environment.NewLine, args[1], ex.Message);
                _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
            }
        }
    }

    private async Task<bool> SaveCurrentDocumentAsync(bool showFileDialog)
    {
        IDocument? doc = Controller.CurrentDocument;
        if (doc is null)
        {
            return true;
        }

        RefreshData();

        if (!doc.Changed)
        {
            return true;
        }

        string? fileName = doc.FileName;

        if (fileName is null)
        {
            showFileDialog = true;
            fileName = $"{Path.GetFileNameWithoutExtension(doc.SourceDocumentFileName)}.{doc.TargetLanguage ?? Res.Language}{App.TsltnFileExtension}";
        }

        if (showFileDialog)
        {
            if (!GetTsltnOutFileName(ref fileName))
            {
                return false;
            }
        }

        var task = Task.Run(() => Controller.CurrentDocument?.Save(fileName));
        _tasks.Add(task);

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            string errorMessage = string.Format(CultureInfo.InvariantCulture, "The file {0}{1}could not be saved:{1}{2}",
                                                fileName, Environment.NewLine, ex.Message);
            _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
            return false;
        }

        return true;
    }

    private async Task<bool> CloseCurrentDocumentAsync()
    {
        if (Controller.CurrentDocument is null)
        {
            return true;
        }

        RefreshData();

        if (Controller.CurrentDocument.Changed)
        {
            MessageBoxResult result = MessageBox.Show(this,
                string.Format("The current document contains changes.{0}Do you want to save these changes?", Environment.NewLine),
                App.ProgramName,
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (result == MessageBoxResult.Yes)
            {
                if (!await SaveCurrentDocumentAsync(false))
                {
                    return false;
                }
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return false;
            }
        }

        Controller.CloseCurrentDocument();
        return true;
    }


    private async Task<bool> ChangeSourceDocumentAsync()
    {
        IDocument? doc = Controller.CurrentDocument;

        if (doc is null)
        {
            return true;
        }

        string? sourceFilePath = Controller.CurrentDocument?.SourceDocumentFileName;

        if (!GetXmlInFileName(ref sourceFilePath))
        {
            return false;
        }

        doc.ChangeSourceDocument(sourceFilePath);

        if (!await SaveCurrentDocumentAsync(false))
        {
            return false;
        }

        Debug.Assert(doc.FileName != null);

        try
        {
            await Task.Run(() => Controller.OpenTsltnDocument(doc.FileName)).ConfigureAwait(false);

        }
        catch (Exception ex)
        {
            string errorMessage = string.Format(CultureInfo.InvariantCulture, Res.OpenFileFailed, Environment.NewLine, doc.FileName, ex.Message);
            _ = Dispatcher.BeginInvoke(() => ShowMessage(errorMessage, MessageBoxImage.Error), DispatcherPriority.Send);
        }

        return true;
    }


    private async Task TranslateCurrentDocumentAsync()
    {
        IDocument? doc = Controller.CurrentDocument;
        if (doc is null || !await SaveCurrentDocumentAsync(false))
        {
            return;
        }

        string? fileName = doc.SourceDocumentFileName;

        if (GetXmlOutFileName(ref fileName))
        {
            (IList<DataError> Errors, IList<KeyValuePair<long, string>> UnusedTranslations) result;
            var task = Task.Run(() => doc.Translate(fileName));
            _tasks.Add(task);
            try
            {
                result = await task.ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, MessageBoxImage.Error);
                return;
            }

            if (result.Errors.Count != 0)
            {
                TranslationError?.Invoke(this, new DataErrorEventArgs(result.Errors));
            }

            if (result.UnusedTranslations.Count != 0)
            {
                var wnd = new SelectUnusedTranslationsWindow(System.IO.Path.GetFileName(Controller.CurrentDocument!.FileName), result.UnusedTranslations);

                if (true == wnd.ShowDialog(this))
                {
                    foreach (UnusedTranslationUserControl cntr in wnd.Controls)
                    {
                        if (cntr.Remove)
                        {
                            doc.RemoveTranslation(cntr.Kvp.Key);
                        }
                    }
                }
            }
        }
    }

}
