using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FolkerKinzel.Tsltn.Controllers
{
    public sealed partial class FileController : INotifyPropertyChanged, IFileController
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        //public event EventHandler<MessageEventArgs>? Message;
        //public event EventHandler<DataErrorEventArgs>? TranslationError;
        //public event EventHandler<UnusedTranslationEventArgs>? UnusedTranslations;
        //public event EventHandler<FileSystemEventArgs>? SourceDocumentDeleted;


        private void OnPropertyChanged([CallerMemberName] string propName = "" ) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));


        //private void OnMessage(MessageEventArgs args) => this.Message?.Invoke(this, args);
    }
}
