using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using FolkerKinzel.Tsltn.Models.Intls;

namespace FolkerKinzel.Tsltn.Models
{
    public partial class Document : IDocument, IFileAccess, IDocumentNodes
    {
        public bool Changed => _tsltn.Changed;

        public string? SourceDocumentFileName
        {
            get
            {
                return _tsltn.SourceDocumentFileName;
            }

            set
            {
                _tsltn.SourceDocumentFileName = value;
                OnPropertyChanged();
            }
        }

        public string? SourceLanguage
        {
            get
            {
                return _tsltn?.SourceLanguage;
            }

            set
            {
                if (_tsltn != null)
                {
                    value = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (!StringComparer.Ordinal.Equals(_tsltn.SourceLanguage, value))
                    {
                        _tsltn.SourceLanguage = value;
                        OnPropertyChanged();
                    }
                }
            }
        }


        public string? TargetLanguage
        {
            get
            {
                return _tsltn?.TargetLanguage;
            }

            set
            {
                if (_tsltn != null)
                {
                    value = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (!StringComparer.Ordinal.Equals(_tsltn.TargetLanguage, value))
                    {
                        _tsltn.TargetLanguage = value;
                        OnPropertyChanged();
                    }
                }
            }
        }

        public INode? FirstNode { get; private set; }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<KeyValuePair<long, string>> GetAllTranslations() => _tsltn.GetAllTranslations();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveTranslation(long id) => SetTranslation(id, null);

    }
}
