using FolkerKinzel.Tsltn.Controllers.Resources;
using FolkerKinzel.Tsltn.Models;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FolkerKinzel.Tsltn.Controllers
{
    public partial class FileController : INotifyPropertyChanged, IFileController
    {
        private readonly IDocument _doc;

        public const string TSLTN_FILE_EXTENSION = ".tsltn";

        

        public FileController(IDocument document)
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

                if (filename is null)
                {
                    this.OnRefreshData();
                    return $"{System.IO.Path.GetFileNameWithoutExtension(_doc.SourceDocumentFileName)}.{_doc.TargetLanguage ?? "<Language>"}{TSLTN_FILE_EXTENSION}";
                }

                return filename ?? "";
            }
        }

        #endregion


        #region Methods

        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        public async Task<bool> DoCloseTsltnAsync()
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
                            await DoSaveAsync(FileName).ConfigureAwait(true);
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        return false;
                }
            }

            OnHasContentChanged(false);
            _doc.Close();
            OnPropertyChanged(nameof(FileName));

            return true;
        }



        [SuppressMessage("Globalization", "CA1303:Literale nicht als lokalisierte Parameter übergeben", Justification = "<Ausstehend>")]
        public async Task<bool> DoSaveAsync(string? fileName)
        {
            if (fileName is null || _doc.TsltnFileName is null)
            {
                fileName = _doc.TsltnFileName ?? this.FileName;
                if (!GetTsltnOutFileName(ref fileName))
                {
                    return false;
                }
            }

            OnRefreshData();

            try
            {
                var task = Task.Run(() => _doc.SaveTsltnAs(fileName));
                this.Tasks.Add(task);
                await task.ConfigureAwait(false);

                OnNewFileName(FileName);
            }
            catch (AggregateException e)
            {
                OnMessage(new MessageEventArgs(e.Message, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));
                OnPropertyChanged(nameof(FileName));

                return false;
            }

            OnPropertyChanged(nameof(FileName));
            return true;
        }


        [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
        public async Task DoOpenAsync(string? fileName)
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
                                await DoSaveAsync(FileName).ConfigureAwait(true);
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
                if (!GetTsltnInFileName(ref fileName))
                {
                    return;
                }
            }

            try
            {
                if (!await Task.Run(() => _doc.Open(fileName)).ConfigureAwait(true))
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
                            _doc.Close();
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
                _doc.Close();
                OnPropertyChanged(nameof(FileName));
                OnBadFileName(fileName);
            }
        }


        public async Task DoNewTsltnAsync()
        {
            await DoCloseTsltnAsync().ConfigureAwait(true);

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

                        _doc.Close();
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
            if (!await DoSaveAsync(_doc.TsltnFileName).ConfigureAwait(true))
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
                        _doc.Close();
                        OnPropertyChanged(nameof(FileName));
                    }

                    return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
                }//catch


            }//if

            return (Array.Empty<DataError>(), Array.Empty<KeyValuePair<long, string>>());
        }


        public Task<(List<DataError> Errors, List<KeyValuePair<long, string>> UnusedTranslations)> DoTranslateAsync(string fileName)
        {
            return Task.Run(() =>
            {
                _doc.Translate(fileName, Res.InvalidXml, out List<DataError> errors, out List<KeyValuePair<long, string>> unusedTranslations);
                return (Errors: errors, UnusedTranslations: unusedTranslations);
            });
        }


        public void RemoveUnusedTranslations(IEnumerable<long> unusedTranslations)
        {
            if (unusedTranslations is null)
            {
                throw new ArgumentNullException(nameof(unusedTranslations));
            }

            foreach (var id in unusedTranslations)
            {
                _doc.RemoveTranslation(id);
            }
        }


        #endregion

    }
}
