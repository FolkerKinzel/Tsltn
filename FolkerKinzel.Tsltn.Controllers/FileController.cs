﻿using FolkerKinzel.Tsltn.Controllers.Resources;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FolkerKinzel.Tsltn.Controllers
{
    [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    public sealed partial class FileController : INotifyPropertyChanged, IFileController, IDisposable
    {
        private IFileAccess? _doc;
        private static FileController? _instance;
        public const string TsltnFileExtension = ".tsltn";

        private FileController()
        {

        }

        public static FileController Instance
        {
            get
            {
                _instance ??= new FileController();
                return _instance;
            }
        }


        #region Properties

        public IFileAccess? CurrentDocument
        {
            get => _doc;
            set
            {
                _doc = value;
                OnPropertyChanged();
            }
        }

         IDocument? IFileController.CurrentDocument => _doc;


        

        #endregion


        #region Methods

        public void Dispose() => _doc?.Dispose();

        public async Task<bool> CloseDocumentAsync() => await CloseTsltnAsync(true).ConfigureAwait(false);


        private async Task<bool> CloseTsltnAsync(bool handleChanges)
        {
            IFileAccess? doc = CurrentDocument;
            if (doc is null)
            {
                return true;
            }

            if (handleChanges)
            {
                OnRefreshData();

                if (doc.Changed)
                {
                    var args = new MessageEventArgs(
                        string.Format(CultureInfo.CurrentCulture, Res.FileWasChanged, System.IO.Path.GetFileName(doc.FileName), Environment.NewLine),
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                    OnMessage(args);

                    switch (args.Result)
                    {
                        case MessageBoxResult.Yes:
                            {
                                if (!await SaveDocumentAsync().ConfigureAwait(true))
                                {
                                    return false;
                                }
                            }
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            return false;
                    }
                }
            }

            doc.Dispose();
            CurrentDocument = null;

            return true;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SaveDocumentAsync() => DoSaveTsltnAsync(_doc?.FileName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SaveAsTsltnAsync() => DoSaveTsltnAsync(null);

        public Task OpenTsltnFromCommandLineAsync(string commandLineArg)
        {
            try
            {
                commandLineArg = Path.GetFullPath(commandLineArg);
            }
            catch (Exception e)
            {
                OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));

                return Task.CompletedTask;
            }

            return LoadDocumentAsync(commandLineArg);
        }

        public async Task LoadDocumentAsync(string? fileName)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(fileName, CurrentDocument?.FileName))
            {
                OnMessage(new MessageEventArgs(Res.FileAlreadyOpen, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK));
                return;
            }

            if (!await CloseDocumentAsync().ConfigureAwait(true))
            {
                return;
            }

            if (fileName is null)
            {
                if (!GetTsltnInFileName(out fileName))
                {
                    return;
                }
            }

            try
            {
                await Task.Run(() => { CurrentDocument = Document.Load(fileName); }).ConfigureAwait(false);
                Debug.Assert(_doc != null);
                Debug.Assert(CurrentDocument != null);
                if (!CurrentDocument.HasSourceDocument)
                {
                    OnMessage(new MessageEventArgs(
                            string.Format(CultureInfo.CurrentCulture, Res.SourceDocumentNotFound, Environment.NewLine, _doc.SourceDocumentFileName),
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK));

                    string? xmlFileName = null;
                    try
                    {
                        xmlFileName = System.IO.Path.GetFileName(CurrentDocument.SourceDocumentFileName);
                    }
                    catch { }

                    if (!GetXmlInFileName(ref xmlFileName))
                    {
                        _ = await CloseDocumentAsync().ConfigureAwait(false);
                        return;
                    }

                    _doc.SourceDocumentFileName = xmlFileName;
                    if (await SaveDocumentAsync().ConfigureAwait(false))
                    {
                        await LoadDocumentAsync(fileName).ConfigureAwait(false);
                    }
                    else
                    {
                        _ = await CloseDocumentAsync().ConfigureAwait(false);
                    }
                    return;
                }

                Debug.Assert(CurrentDocument != null);

                if (!CurrentDocument.HasValidSourceDocument)
                {
                    OnMessage(new MessageEventArgs(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Res.EmptyOrInvalidFile,
                            Environment.NewLine, System.IO.Path.GetFileName(CurrentDocument.SourceDocumentFileName), Res.XmlDocumentationFile),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK));
                }
            }
            catch (Exception e)
            {
                OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                _ = await CloseTsltnAsync(false).ConfigureAwait(true);
                OnBadFileName(fileName);
            }
        }


        public async Task NewDocumentAsync()
        {
            if (!await CloseDocumentAsync().ConfigureAwait(false))
            {
                return;
            }

            string? xmlFileName = null;

            if (GetXmlInFileName(ref xmlFileName))
            {
                try
                {
                    _ = await Task.Run(() => CurrentDocument = Document.Create(xmlFileName)).ConfigureAwait(false);
                    Debug.Assert(CurrentDocument != null);

                    if (!CurrentDocument.HasValidSourceDocument)
                    {
                        OnMessage(new MessageEventArgs(
                            string.Format(CultureInfo.CurrentCulture, Res.EmptyOrInvalidFile, Environment.NewLine, xmlFileName, Res.XmlDocumentationFile),
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK));

                        _ = await CloseTsltnAsync(false).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                }
            }

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SuspendSourceFileObservation() => _watcher.RaiseEvents = false;

        public void ResumeSourceFileObservation()
        {
            if (_watcher.WatchedFile != CurrentDocument?.SourceDocumentFileName)
            {
                FileWatcher_Reload(this, EventArgs.Empty);
            }
            _watcher.RaiseEvents = true;
        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public async Task<(IEnumerable<DataError> Errors, IEnumerable<KeyValuePair<long, string>> UnusedTranslations)> TranslateAsync()
        {
            //await _doc.WaitAllTasks().ConfigureAwait(false);

            IFileAccess? doc = CurrentDocument;
            if (doc is null || !await DoSaveTsltnAsync(doc.FileName).ConfigureAwait(false))
            {
                return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
            }

            if (GetXmlOutFileName(out string? fileName))
            {
                try
                {
                    return await Task.Run(() =>
                    {
                        doc.Translate(fileName, out List<DataError> errors, out List<KeyValuePair<long, string>> unusedTranslations);
                        return (Errors: errors, UnusedTranslations: unusedTranslations);
                    }).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                }//catch
            }//if

            return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());

        }

        public Task ChangeSourceDocumentAsync(string? newSourceDocument)
        {
            if (_doc != null)
            {
                _doc.SourceDocumentFileName = newSourceDocument;

                return ReloadTsltnAsync();
            }

            return Task.CompletedTask;
        }



        #endregion

    }
}
