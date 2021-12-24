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

namespace FolkerKinzel.Tsltn.Controllers
{
    [SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>")]
    public sealed partial class FileController : INotifyPropertyChanged, IFileController, IDisposable
    {
        private IFileAccess? _doc;
        private static FileController? _instance;
        public const string TsltnFileExtension = ".tsltn";

        public event PropertyChangedEventHandler? PropertyChanged;

        private FileController() { }

        #region Properties

        public static FileController Instance
        {
            get
            {
                _instance ??= new FileController();
                return _instance;
            }
        }


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

                OnPropertyChanged();
            }
        }

        IDocument? IFileController.CurrentDocument => _doc;

        #endregion


        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => _doc?.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CloseCurrentDocument() => CurrentDocument = null;
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenTsltnDocument(string tsltnFileName) => CurrentDocument = Document.Load(tsltnFileName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NewDocument(string xmlFileName) => CurrentDocument = Document.Create(xmlFileName);

        private void OnPropertyChanged([CallerMemberName] string propName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #endregion

    }
}
