using FolkerKinzel.Tsltn.Controllers.Resources;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolkerKinzel.Tsltn.Controllers
{
    public partial class FileController : INotifyPropertyChanged, IFileController
    {
        private readonly IFileAccess _doc;

        public const string TSLTN_FILE_EXTENSION = ".tsltn";

        

        public FileController(IFileAccess document)
        {
            this._doc = document;
        }


        #region Properties


        public ConcurrentBag<Task> Tasks { get; } = new ConcurrentBag<Task>();


        public string FileName
        {
            get
            {
                string? filename = _doc.TsltnFileName;

                if (filename is null && _doc.SourceDocumentFileName != null)
                {
                    this.OnRefreshData();
                    return $"{System.IO.Path.GetFileNameWithoutExtension(_doc.SourceDocumentFileName)}.{_doc.TargetLanguage ?? Res.Language}{TSLTN_FILE_EXTENSION}";
                }

                return filename ?? "";
            }
        }

        #endregion


        #region Methods

        public async Task<bool> CloseTsltnAsync()
        {
            if (_doc.SourceDocumentFileName is null)
            {
                return true;
            }

            OnRefreshData();

            if (_doc.Changed)
            {
                var args = new MessageEventArgs(
                    string.Format(CultureInfo.CurrentCulture, Res.FileWasChanged, System.IO.Path.GetFileName(this.FileName), Environment.NewLine),
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                OnMessage(args);

                switch (args.Result)
                {
                    case MessageBoxResult.Yes:
                        {
                            await DoSaveTsltnAsync(FileName).ConfigureAwait(true);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return false;
                }
            }

            OnHasContentChanged(false);
            _doc.CloseTsltn();
            OnPropertyChanged(nameof(FileName));

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SaveTsltnAsync() => DoSaveTsltnAsync(_doc.TsltnFileName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<bool> SaveAsTsltnAsync() => DoSaveTsltnAsync(null);


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public async Task OpenTsltnAsync(string? fileName)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(fileName, this.FileName))
            {
                OnMessage(new MessageEventArgs(Res.FileAlreadyOpen, MessageBoxButton.OK, MessageBoxImage.Asterisk, MessageBoxResult.OK));
                return;
            }

            if (_doc.SourceDocumentFileName != null)
            {
                OnRefreshData();

                if (_doc.Changed)
                {
                    var arg = new MessageEventArgs(
                        string.Format(CultureInfo.CurrentCulture, Res.FileWasChanged, System.IO.Path.GetFileName(this.FileName), Environment.NewLine),
                        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                    OnMessage(arg);

                    switch (arg.Result)
                    {
                        case MessageBoxResult.Yes:
                            {
                                await DoSaveTsltnAsync(FileName).ConfigureAwait(true);
                            }
                            break;
                        case MessageBoxResult.No:
                            break;
                        default:
                            return;
                    }
                }
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
                if (!await Task.Run(() => _doc.OpenTsltn(fileName)).ConfigureAwait(true))
                {
                    OnHasContentChanged(false);
                    OnPropertyChanged(nameof(FileName));
                    OnMessage(new MessageEventArgs(
                            string.Format(CultureInfo.CurrentCulture, Res.SourceDocumentNotFound, Environment.NewLine, _doc.SourceDocumentFileName),
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK));


                    string? xmlFileName = null;
                    try
                    {
                        xmlFileName = System.IO.Path.GetFileName(_doc.SourceDocumentFileName);
                    }
                    catch { }

                    do
                    {
                        if (!GetXmlInFileName(ref xmlFileName))
                        {
                            OnHasContentChanged(false);
                            _doc.CloseTsltn();
                            OnPropertyChanged(nameof(FileName));

                            return;
                        }


                    } while (!_doc.ReloadSourceDocument(xmlFileName));
                }


                if (_doc.FirstNode is null)
                {
                    OnHasContentChanged(false);

                    OnMessage(new MessageEventArgs(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Res.EmptyOrInvalidFile,
                            Environment.NewLine, System.IO.Path.GetFileName(_doc.SourceDocumentFileName), Res.XmlDocumentationFile),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information,
                        MessageBoxResult.OK));
                }
                else
                {
                    OnHasContentChanged(true);
                }

                OnPropertyChanged(nameof(FileName));
                OnNewFileName(FileName);
            }
            catch (AggregateException e)
            {
                OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                OnHasContentChanged(false);
                _doc.CloseTsltn();
                OnPropertyChanged(nameof(FileName));
                OnBadFileName(fileName);
            }
        }


        public async Task NewTsltnAsync()
        {
            await CloseTsltnAsync().ConfigureAwait(true);

            string? xmlFileName = null;

            if (GetXmlInFileName(ref xmlFileName))
            {
                try
                {
                    await Task.Run(() => _doc.NewTsltn(xmlFileName)).ConfigureAwait(true);

                    if (_doc.FirstNode is null)
                    {
                        OnMessage(new MessageEventArgs(
                            string.Format(CultureInfo.CurrentCulture, Res.EmptyOrInvalidFile, Environment.NewLine, xmlFileName, Res.XmlDocumentationFile),
                            MessageBoxButton.OK,
                            MessageBoxImage.Exclamation,
                            MessageBoxResult.OK));

                        _doc.CloseTsltn();
                    }
                    else
                    {
                        OnHasContentChanged(true);
                    }

                }
                catch (AggregateException ex)
                {
                    OnMessage(new MessageEventArgs(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                }
                finally
                {
                    OnPropertyChanged(nameof(FileName));
                }

            }

        }

        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public async Task<(IEnumerable<DataError> Errors, IEnumerable<KeyValuePair<long, string>> UnusedTranslations)> TranslateAsync()
        {
            try
            {
                await Task.WhenAll(this.Tasks.ToArray()).ConfigureAwait(true);
            }
            catch { }
            
            this.Tasks.Clear();

            if (!await DoSaveTsltnAsync(_doc.TsltnFileName).ConfigureAwait(true))
            {
                return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
            }

            if (GetXmlOutFileName(out string? fileName))
            {
                try
                {
                    return await Task.Run(() =>
                    {
                        _doc.Translate(fileName, Res.InvalidXml, out List<DataError> errors, out List<KeyValuePair<long, string>> unusedTranslations);
                        return (Errors: errors, UnusedTranslations: unusedTranslations);
                    }).ConfigureAwait(false);

                    
                }
                catch (AggregateException ex)
                {
                    OnMessage(new MessageEventArgs(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));

                    try
                    {
                        await Task.Run(() => _doc.ReloadSourceDocument(_doc.SourceDocumentFileName!)).ConfigureAwait(false);
                    }
                    catch
                    {
                        OnHasContentChanged(false);
                        _doc.CloseTsltn();
                        OnPropertyChanged(nameof(FileName));
                    }

                    return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
                }//catch


            }//if

            return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
        }


        #endregion

    }
}
