using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess
    {
        bool Changed { get; }

        void Save(string tsltnFileName);

        //bool ReloadSourceDocument(string fileName);

        string? SourceDocumentFileName { get; set; }

        void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations);


    }
}
