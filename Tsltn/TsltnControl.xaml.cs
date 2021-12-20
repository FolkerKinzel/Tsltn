﻿using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using FolkerKinzel.Tsltn.Models;
using Tsltn.Resources;

namespace Tsltn
{
    /// <summary>
    /// Interaktionslogik für TsltnControl.xaml
    /// </summary>
    public sealed partial class TsltnControl : UserControl, INotifyPropertyChanged, IDisposable
    {
        private readonly MainWindow _owner;
        private INode _node;
        private string _translation = "";
        private readonly IDocument _doc;
        private bool _hasTranslation;
        private string? _sourceLanguage;
        private string? _targetLanguage;
        private INode? _nextUntranslatedNode;
        private readonly ConcurrentBag<Task> _tasks = new ConcurrentBag<Task>();

        private readonly StringBuilder _sb = new StringBuilder(1024);
        private readonly object _lockObject = new object();


        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public event PropertyChangedEventHandler? PropertyChanged;

        private static readonly DataError MissingTranslationWarning = new DataError(ErrorLevel.Warning, Res.UntranslatedElement, null);
        private static readonly DataError InvalidSourceLanguage = new DataError(ErrorLevel.Error, Res.InvalidSourceLanguage, null);
        private static readonly DataError InvalidTargetLanguage = new DataError(ErrorLevel.Error, Res.InvalidTargetLanguage, null);
        private static readonly DataError MissingSourceLanguage = new DataError(ErrorLevel.Information, Res.SourceLanguageNotSpecified, null);
        private static readonly DataError MissingTargetLanguage = new DataError(ErrorLevel.Information, Res.TargetLanguageNotSpecified, null);


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208:Argumentausnahmen korrekt instanziieren", Justification = "<Ausstehend>")]
        public TsltnControl(MainWindow owner, IDocument doc)
        {
            if (owner is null || doc?.FirstNode is null)
            {
                throw new ArgumentNullException();
            }

            //this.DataContext = this;

            _owner = owner;
            _doc = doc;
            _node = doc.FirstNode!;

            if (_node.Translation != null)
            {
                _translation = _node.Translation;
                _hasTranslation = true;
            }

            InitializeComponent();

            NavCtrl.NavigationRequested += NavCtrl_NavigationRequested;

            _owner.TranslationErrors += MainWindow_TranslationErrors;

            DataObject.AddPastingHandler(_tbTranslation, TbTranslation_Paste);

            _btnPrevious.ToolTip = $"{Res.AltKey}+{Res.LeftKey}";
            _btnNext.ToolTip = $"{Res.AltKey}+{Res.RightKey}";
            _btnNextToTranslate.ToolTip = $"{Res.ShiftKey}+{Res.AltKey}+{Res.RightKey}";
            _btnFirstNode.ToolTip = $"{Res.AltKey}+{Res.Pos1Key}";
        }


        public bool HasTranslation
        {
            get => _hasTranslation;
            set
            {
                if (value != _hasTranslation)
                {
                    _hasTranslation = value;
                    OnPropertyChanged();

                    if (!value)
                    {
                        Translation = "";
                    }

                    _tasks.Add(CheckUntranslatedNodesAsync());
                }
            }
        }



        [AllowNull]
        public string Translation
        {
            get => _translation;
            set
            {
                _translation = value ?? "";
                OnPropertyChanged();
            }
        }


        public string? SourceLanguage
        {
            get => _sourceLanguage;
            set
            {
                Errors.Remove(MissingSourceLanguage);

                if (string.IsNullOrWhiteSpace(value))
                {
                    _sourceLanguage = null;
                    OnPropertyChanged();
                    this.Errors.Insert(0, MissingSourceLanguage);
                }
                else
                {
                    _sourceLanguage = value.Replace(" ", "", StringComparison.Ordinal);
                    OnPropertyChanged();

                    // wirft ggf. CultureNotFoundException
                    _ = CultureInfo.GetCultureInfoByIetfLanguageTag(_sourceLanguage);
                }
            }
        }


        public string? TargetLanguage
        {
            get => _targetLanguage;
            set
            {
                Errors.Remove(MissingTargetLanguage);

                if (string.IsNullOrWhiteSpace(value))
                {
                    _targetLanguage = null;
                    OnPropertyChanged();
                    Errors.Insert(0, MissingTargetLanguage);
                }
                else
                {
                    _targetLanguage = value.Replace(" ", "", StringComparison.Ordinal);
                    OnPropertyChanged();

                    // wirft ggf. CultureNotFoundException
                    _ = CultureInfo.GetCultureInfoByIetfLanguageTag(_targetLanguage);
                }
            }
        }


        internal void RefreshSourceFileName() => OnPropertyChanged(nameof(SourceFileName));

        public string? SourceFileName => _doc.SourceDocumentFileName;

        public INode CurrentNode
        {
            get => _node;

            set
            {
                _node = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<DataError> Errors { get; } = new ObservableCollection<DataError>();


        public void Dispose() => ((IDisposable)_cancellationTokenSource).Dispose();


        internal void UpdateSource()
        {
            _node.Translation = HasTranslation ? Translation.Trim() : null;


            if (!Validation.GetHasError(_tbSourceLanguage))
            {
                if (SourceLanguage != _doc.SourceLanguage)
                {
                    _doc.SourceLanguage = SourceLanguage;
                }
            }


            if (!Validation.GetHasError(_tbTargetLanguage))
            {
                if (TargetLanguage != _doc.TargetLanguage)
                {
                    _doc.TargetLanguage = TargetLanguage;
                }
            }
        }

        #region EventHandler

        private void TsltnControl_Loaded(object sender, RoutedEventArgs e)
        {
            SourceLanguage = _doc.SourceLanguage;
            TargetLanguage = _doc.TargetLanguage;


            // Möglichst nicht INNERHALB des Loaded-Eventhandlers auf den VisualTree
            // zugreifen: Das führt zu schwer identifizierbaren Fehlern:
            _ = Dispatcher.BeginInvoke(new Action(async () =>
            {
                await CheckUntranslatedNodesAsync().ConfigureAwait(true);
                CommandManager.InvalidateRequerySuggested();
                _ = Task.Run(() => CheckXmlError(_cancellationTokenSource.Token));
            }), DispatcherPriority.ApplicationIdle);

            Clipboard.Clear();
        }


        private async void TsltnControl_Unloaded(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();

            await Task.WhenAll(_tasks).ConfigureAwait(false);

            _owner.TranslationErrors -= MainWindow_TranslationErrors;
            DataObject.RemovePastingHandler(_tbTranslation, TbTranslation_Paste);

            Dispose();
        }

        private void MainWindow_TranslationErrors(object? sender, TranslationErrorsEventArgs e)
        {
            if (e.Errors.Any())
            {
                this.Errors.Clear();

                foreach (DataError error in e.Errors)
                {
                    Errors.Add(error);
                }

                if (SourceLanguage is null)
                {
                    Errors.Add(MissingSourceLanguage);
                }
                else if (Validation.GetHasError(_tbSourceLanguage))
                {
                    Errors.Add(InvalidSourceLanguage);
                }

                if (TargetLanguage is null)
                {
                    Errors.Add(MissingTargetLanguage);
                }
                else if (Validation.GetHasError(_tbTargetLanguage))
                {
                    Errors.Add(InvalidTargetLanguage);
                }

                _tasks.Add(CheckUntranslatedNodesAsync());
            }
        }


        private void DataError_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((e.OriginalSource as FrameworkElement)?.DataContext is DataError err)
            {
                if (object.ReferenceEquals(err, InvalidSourceLanguage) ||
                    object.ReferenceEquals(err, MissingSourceLanguage))
                {
                    _ = Keyboard.Focus(_tbSourceLanguage);
                }
                else if (object.ReferenceEquals(err, InvalidTargetLanguage) ||
                    object.ReferenceEquals(err, MissingTargetLanguage))
                {
                    _ = Keyboard.Focus(_tbTargetLanguage);
                }
                else
                {
                    Navigate(err.Node);
                    //Keyboard.Focus(control._tbTranslation);
                }
            }
        }


        private void Language_Error(object sender, ValidationErrorEventArgs e)
        {
            if (e.OriginalSource is TextBox tb)
            {
                if (tb.Name == nameof(_tbSourceLanguage))
                {
                    Errors.Remove(InvalidSourceLanguage);

                    if (Validation.GetHasError(_tbSourceLanguage))
                    {
                        Errors.Insert(0, InvalidSourceLanguage);
                    }
                }
                else
                {
                    Errors.Remove(InvalidTargetLanguage);

                    if (Validation.GetHasError(_tbTargetLanguage))
                    {
                        Errors.Insert(0, InvalidTargetLanguage);
                    }
                }
            }
        }


        private void NavCtrl_NavigationRequested(object? sender, NavigationRequestedEventArgs e)
        {
            INode? target = _doc.FirstNode?.FindNode(e.PathFragment, !e.CaseSensitive, e.WholeWord);

            if (!(target is null))
            {
                Navigate(target);
                NavCtrl.SetComboBoxItem(e.PathFragment);
            }
        }




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            HasTranslation = false;
            _ = _btnBrowseAll.Focus();
            //_btnNext.Focus();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void TbTranslation_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => HasTranslation = true;


        private void TbTranslation_Paste(object sender, DataObjectPastingEventArgs e)
        {
            e.CancelCommand();
            _sb.Clear().Append(Clipboard.GetText())
                .Replace("<c> null </c>", "<c>null</c>")
                .Replace("<c> true </c>", "<c>true</c>")
                .Replace("<c> false </c>", "<c>false</c>");
            //Clipboard.Clear();

            int markupCounter = 0;
            char previous = 'a';



            for (int i = _sb.Length - 1; i >= 0; i--)
            {
                if (markupCounter < 0)
                {
                    break;
                }

                char current = _sb[i];

                switch (current)
                {
                    case '>':
                        markupCounter++;
                        break;
                    case '<':
                        markupCounter--;
                        break;
                    default:
                        if (markupCounter > 0) // inside Markup
                        {
                            if (char.IsWhiteSpace(current) && char.IsPunctuation(previous)) // das '=' - Zeichen ist nicht Punctuation
                            {
                                _ = _sb.Remove(i, 1);
                                continue;
                            }
                            else if (char.IsPunctuation(current) && char.IsWhiteSpace(previous))
                            {
                                _ = _sb.Remove(i + 1, 1);
                            }
                        }
                        break;
                }

                previous = current;
            }

            if (_tbTranslation.IsSelectionActive)
            {
                string replacement = _sb.ToString();
                _sb.Clear().Append(_tbTranslation.Text);

                int selectionStart = _tbTranslation.SelectionStart;
                _sb.Remove(selectionStart, _tbTranslation.SelectionLength);
                _sb.Insert(_tbTranslation.SelectionStart, replacement);


                Translation = _sb.ToString();

                _tbTranslation.Select(selectionStart + replacement.Length, 0);
            }
        }


        #endregion


        #region Commands

        private void PreviousPage_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = CurrentNode.HasAncestor;

        private void BrowseHome_Executed(object sender, ExecutedRoutedEventArgs e) => Navigate(_doc.FirstNode);

        private async void PreviousPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            INode? ancestor = await Task.Run(() => _node.GetAncestor()).ConfigureAwait(true);
            Navigate(ancestor);
        }

        private void NextPage_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = CurrentNode.HasDescendant;

        private async void NextPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            INode? descendant = await Task.Run(() => CurrentNode.GetDescendant()).ConfigureAwait(true);
            Navigate(descendant);
        }

        private void CopyXml_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(CurrentNode.InnerXml);
            //e.Handled = true;
            //_btnNext.Focus();

            _ = _tbOriginal.Focus();
        }


        private void BrowseAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var allWnd = new BrowseAllTranslationsWindow(_doc.GetAllTranslations());

            if (true == allWnd.ShowDialog(_owner))
            {
                if (allWnd.TextCopied)
                {
                    if (_tbTranslation.SelectionStart == 0)
                    {
                        _tbTranslation.SelectionStart = _tbTranslation.Text.Length;
                    }

                    _ = _tbTranslation.Focus();
                    return;
                }
                else if (allWnd._lbTranslations.SelectedItem is string s)
                {
                    HasTranslation = true;
                    Translation = s;
                }
            }

            _ = _btnNext.Focus();
        }

        private void NextToTranslate_CanExecute(object sender, CanExecuteRoutedEventArgs e) => e.CanExecute = _nextUntranslatedNode != null;

        private async void NextToTranslate_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HasTranslation)
            {
                Navigate(_nextUntranslatedNode);
            }
            else
            {
                INode? next = await Task.Run(CurrentNode.GetNextUntranslated).ConfigureAwait(true);
                Navigate(next);
            }

            _ = _btnNextToTranslate.Focus();
        }

        #endregion


        #region private

        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        private async Task CheckUntranslatedNodesAsync()
        {
            UpdateSource();
            _nextUntranslatedNode = HasTranslation ? await Task.Run(CurrentNode.GetNextUntranslated).ConfigureAwait(true) : CurrentNode;


            if (_nextUntranslatedNode != null)
            {
                MissingTranslationWarning.Node = _nextUntranslatedNode;

                if (!Errors.Contains(MissingTranslationWarning))
                {
                    Errors.Insert(0, MissingTranslationWarning);
                }
            }
            else
            {
                _ = Errors.Remove(MissingTranslationWarning);
            }
        }


        private void CheckXmlError(CancellationToken cancelToken)
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(5000);

                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!_owner.IsCommandEnabled)
                    {
                        continue;
                    }


                    if (!HasTranslation)
                    {
                        Dispatcher.Invoke(() => RemoveXmlErrorMessages(), DispatcherPriority.SystemIdle, CancellationToken.None);
                        continue;
                    }


                    if (!IsValidXml(Translation, out string? exceptionMessage))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            RemoveXmlErrorMessages();
                            Errors.Insert(0, new XmlDataError(CurrentNode, exceptionMessage));
                        }, DispatcherPriority.SystemIdle, CancellationToken.None);
                    }
                    else
                    {
                        Dispatcher.Invoke(() => RemoveXmlErrorMessages(), DispatcherPriority.SystemIdle, CancellationToken.None);
                    }


                    if (cancelToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (TaskCanceledException) { }

            Debug.WriteLine("CheckXmlError beendet.");
        }

        private bool IsValidXml(string translation, [NotNullWhen(false)] out string? exceptionMessage)
        {
            exceptionMessage = null;

            try
            {
                lock (_lockObject)
                {
                    _ = _sb.Clear().Append("<R>").Append(translation).Append("</R>");
                    _ = XElement.Parse(_sb.ToString(), LoadOptions.None);
                }
            }
            catch (XmlException e)
            {
                exceptionMessage = e.Message;
                return false;
            }
            catch(Exception)
            {

            }
            return true;
        }


        private void RemoveXmlErrorMessages()
        {
            DataError[] thisErrors = Errors.Where(x => x is XmlDataError && CurrentNode.ReferencesSameXml(x.Node!)).ToArray();

            foreach (DataError error in thisErrors)
            {
                _ = Errors.Remove(error);
            }
        }


        private void Navigate(INode? node)
        {
            if (node is null || node.ReferencesSameXml(CurrentNode))
            {
                return;
            }

            UpdateSource();

            CurrentNode = node;

            string? transl = node.Translation;
            Translation = transl;

            // Die lokale Variable muss benutzt werden,
            // da Translation nie null zurückgibt.
            HasTranslation = transl != null;
        }




        #endregion


    }
}
