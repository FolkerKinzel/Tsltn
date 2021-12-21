﻿using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess : IDocument, IDisposable
    {
        void Save(string tsltnFileName);

        new string FileName { get; set; }
        
        bool HasSourceDocument { get; }
        

        new string? SourceDocumentFileName { get; set; }

        TranslationsController Translations { get; }

        (IList<DataError> Errors, IList<KeyValuePair<long, string>> UnusedTranslations) Translate(string outFileName);

        //void Translate(
        //    string outFileName,
        //    out List<DataError> errors,
        //    out List<KeyValuePair<long, string>> unusedTranslations);


    }
}
