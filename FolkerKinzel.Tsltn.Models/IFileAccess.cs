using System;
using System.Collections.Generic;
using System.Text;

namespace FolkerKinzel.Tsltn.Models
{
    public interface IFileAccess : IDocument
    {
        string? TsltnFileName { get; }
        void Close();
        void NewTsltn(string sourceDocumentFileName);
        bool Open(string? tsltnFileName);
        void SaveTsltnAs(string tsltnFileName);

        bool ReloadSourceDocument(string fileName);

        void Translate(
            string outFileName,
            string invalidXml,
            out List<DataError> errors,
            out List<KeyValuePair<long, string>> unusedTranslations);


    }
}
