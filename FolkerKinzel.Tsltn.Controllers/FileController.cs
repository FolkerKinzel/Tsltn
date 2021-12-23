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
                if(ReferenceEquals(_doc, value))
                {
                    return;
                }

                _doc?.Dispose();
                _doc = value;

                if(_doc != null)
                {
                    _doc.SourceDocumentChanged += Document_SourceDocumentChanged;
                    _doc.SourceDocumentDeleted += Document_SourceDocumentDeleted;
                    _doc.FileWatcherFailed += Document_FileWatcherFailed;
                }

                OnPropertyChanged();
            }
        }

        private void Document_FileWatcherFailed(object? sender, ErrorEventArgs e)
        {
            var args = new MessageEventArgs(
                string.Format(CultureInfo.InvariantCulture, Res.FileWatcherFailed, Environment.NewLine),
                MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            OnMessage(args);
        }

        private void Document_SourceDocumentDeleted(object? sender, FileSystemEventArgs e) 
            => SourceDocumentDeleted?.Invoke(sender, e);


        private async void Document_SourceDocumentChanged(object? sender, FileSystemEventArgs e)
        {
            var args = new MessageEventArgs(
                string.Format(CultureInfo.InvariantCulture, Res.SourceDocumentChanged, Environment.NewLine),
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            OnMessage(args);

            if (args.Result == MessageBoxResult.Yes)
            {
                await ReloadDocumentAsync().ConfigureAwait(false);
            }
        }

        IDocument? IFileController.CurrentDocument => _doc;


        

        #endregion


        #region Methods

        public void Dispose() => _doc?.Dispose();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseCurrentDocument() => CurrentDocument = null;
            

        //public Task OpenTsltnFromCommandLineAsync(string commandLineArg)
        //{
        //    try
        //    {
        //        commandLineArg = Path.GetFullPath(commandLineArg);
        //    }
        //    catch (Exception e)
        //    {
        //        OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));

        //        return Task.CompletedTask;
        //    }

        //    return OpenTsltnDocument(commandLineArg);
        //}

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenTsltnDocument(string tsltnFileName) => CurrentDocument = Document.Load(tsltnFileName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewDocument(string xmlFileName) => CurrentDocument = Document.Create(xmlFileName);


        //[SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        //public async Task TranslateAsync()
        //{
        //    IFileAccess? doc = CurrentDocument;
        //    if (doc is null || !await DoSaveTsltnAsync(doc.FileName).ConfigureAwait(false))
        //    {
        //        return;
        //    }

        //    if (GetXmlOutFileName(out string? fileName))
        //    {
        //        (IList<DataError> Errors, IList<KeyValuePair<long, string>> UnusedTranslations) results;
        //        try
        //        {
        //            results = await Task.Run(() =>  doc.Translate(fileName)).ConfigureAwait(true);
        //        }
        //        catch (Exception e)
        //        {
        //            OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
        //            return;
        //        }//catch

        //        if(results.Errors.Count != 0)
        //        {
        //            TranslationError?.Invoke(this, new DataErrorEventArgs(results.Errors));
        //        }

        //        if(results.UnusedTranslations.Count != 0)
        //        {
        //            var args = new UnusedTranslationEventArgs(results.UnusedTranslations);
        //            UnusedTranslations?.Invoke(this, args);

        //            foreach (long id in args.TranslationsToRemove)
        //            {
        //                doc.Translations.RemoveTranslation(id);
        //            }
        //        }



        //    }//if
        //}


        //public Task ChangeSourceDocumentAsync(string? newSourceDocument)
        //{
        //    if (_doc != null)
        //    {
        //        _doc.SourceDocumentFileName = newSourceDocument;

        //        return ReloadDocumentAsync();
        //    }

        //    return Task.CompletedTask;
        //}



        #endregion

    }
}
