using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess : IDocument
    {
        void Save(string tsltnFileName);

        bool ReloadSourceDocument(string fileName);

        void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations);


    }
}
