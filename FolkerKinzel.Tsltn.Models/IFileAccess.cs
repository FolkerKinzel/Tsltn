using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess : IDocument
    {
        //string? TsltnFileName { get;}
        //void CloseTsltn();
        //void NewTsltn(string sourceDocumentFileName);
        //bool OpenTsltn(string? tsltnFileName);
        void SaveTsltnAs(string tsltnFileName);

        bool ReloadSourceDocument(string fileName);

        void Translate(
            string outFileName,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations);


    }
}
